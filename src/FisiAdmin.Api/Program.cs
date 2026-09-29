using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FisiAdmin.Domain;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5080");
builder.Services.AddSingleton<DemoStore>();
builder.Services.AddSingleton<AuthService>();
var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Correlation-Id"] = context.TraceIdentifier;
    if (context.Request.Headers.Authorization.ToString() is { } header && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        var auth = context.RequestServices.GetRequiredService<AuthService>();
        var session = auth.Validate(header[7..]);
        if (session is not null)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, session.UserName), new("display_name", session.DisplayName) };
            claims.AddRange(session.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "FisiSession"));
        }
    }
    await next();
});

var api = app.MapGroup("/api/v1");
api.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTimeOffset.UtcNow }));
api.MapPost("/auth/login", (LoginRequest request, AuthService auth) =>
{
    var result = auth.Login(request.UserName, request.Password);
    return result is null ? Results.Unauthorized() : Results.Ok(result);
});
api.MapPost("/auth/logout", (HttpContext ctx, AuthService auth) => { auth.Logout(GetToken(ctx)); return Results.NoContent(); }).RequireAnyRole();
api.MapGet("/auth/me", (HttpContext ctx) => Results.Ok(new CurrentUser(ctx.User.Identity!.Name!, ctx.User.FindFirst("display_name")?.Value ?? ctx.User.Identity.Name!, ctx.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray()))).RequireAnyRole();

api.MapGet("/dashboard", (DemoStore db) => Results.Ok(new DashboardSummary(18, 19, db.Users.Count(x => x.Enabled), db.Jobs.Count(x => x.Status != "Completed"), 86))).RequireAnyRole();
api.MapGet("/users", (string? search, DemoStore db) =>
{
    var q = db.Users.AsEnumerable();
    if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => $"{x.DisplayName} {x.UserPrincipalName} {x.Department}".Contains(search, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(q);
}).RequireAnyRole();
api.MapGet("/servers", (DemoStore db) => Results.Ok(db.Servers)).RequireAnyRole();
api.MapGet("/jobs", (DemoStore db) => Results.Ok(db.Jobs.OrderByDescending(x => x.CreatedAt))).RequireRoles(AppRoles.PlatformAdmin, AppRoles.AdAdmin, AppRoles.Auditor);
api.MapPost("/users/onboarding", (CreateUserRequest request, DemoStore db, HttpContext ctx) =>
{
    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.UserPrincipalName))
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["user"] = ["Vorname, Nachname und UPN sind Pflichtfelder."] });
    if (db.Users.Any(x => x.UserPrincipalName.Equals(request.UserPrincipalName, StringComparison.OrdinalIgnoreCase))) return Results.Conflict(new { message = "Der UPN existiert bereits." });
    var job = new AdminJob(Guid.NewGuid(), "UserOnboarding", request.UserPrincipalName, "Queued", DateTimeOffset.UtcNow, ctx.User.Identity!.Name!);
    db.Jobs.Add(job); return Results.Accepted($"/api/v1/jobs/{job.Id}", job);
}).RequireRoles(AppRoles.PlatformAdmin, AppRoles.AdAdmin);
api.MapPost("/users/{id:guid}/unlock", (Guid id, DemoStore db, HttpContext ctx) =>
{
    var index = db.Users.FindIndex(x => x.Id == id); if (index < 0) return Results.NotFound();
    var user = db.Users[index]; db.Users[index] = user with { Locked = false };
    var job = new AdminJob(Guid.NewGuid(), "UnlockUser", user.UserPrincipalName, "Completed", DateTimeOffset.UtcNow, ctx.User.Identity!.Name!, "Benutzer entsperrt");
    db.Jobs.Add(job); return Results.Ok(job);
}).RequireRoles(AppRoles.PlatformAdmin, AppRoles.AdAdmin, AppRoles.ServiceDesk);
app.Run();

static string GetToken(HttpContext ctx) => ctx.Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

static class RoleEndpointExtensions
{
    public static RouteHandlerBuilder RequireAnyRole(this RouteHandlerBuilder b) => b.AddEndpointFilter(new RoleFilter([]));
    public static RouteHandlerBuilder RequireRoles(this RouteHandlerBuilder b, params string[] roles) => b.AddEndpointFilter(new RoleFilter(roles));
}
sealed class RoleFilter(string[] roles) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext c, EndpointFilterDelegate next)
    {
        var user = c.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true) return Results.Unauthorized();
        if (roles.Length > 0 && !roles.Any(user.IsInRole)) return Results.Forbid();
        return await next(c);
    }
}
sealed record DemoAccount(string UserName, string DisplayName, string PasswordHash, string[] Roles);
sealed record DemoSession(string UserName, string DisplayName, string[] Roles, DateTimeOffset ExpiresAt);
sealed class AuthService
{
    private readonly ConcurrentDictionary<string, DemoSession> _sessions = new();
    private readonly DemoAccount[] _accounts = [
        New("admin", "Plattformadministrator", "Admin!123", [AppRoles.PlatformAdmin]),
        New("adadmin", "AD Administrator", "AdAdmin!123", [AppRoles.AdAdmin]),
        New("helpdesk", "Service Desk", "Helpdesk!123", [AppRoles.ServiceDesk]),
        New("auditor", "Auditor", "Audit!123", [AppRoles.Auditor]),
        New("readonly", "Nur Lesen", "ReadOnly!123", [AppRoles.ReadOnly])
    ];
    public LoginResponse? Login(string userName, string password)
    {
        var account = _accounts.FirstOrDefault(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        if (account is null || !CryptographicOperations.FixedTimeEquals(Convert.FromHexString(account.PasswordHash), Convert.FromHexString(Hash(password)))) return null;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); var expires = DateTimeOffset.UtcNow.AddHours(8);
        _sessions[token] = new(account.UserName, account.DisplayName, account.Roles, expires);
        return new(token, expires, account.DisplayName, account.Roles);
    }
    public DemoSession? Validate(string token) => _sessions.TryGetValue(token, out var s) && s.ExpiresAt > DateTimeOffset.UtcNow ? s : null;
    public void Logout(string token) => _sessions.TryRemove(token, out _);
    private static DemoAccount New(string u, string d, string p, string[] r) => new(u, d, Hash(p), r);
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
sealed class DemoStore
{
    public List<DirectoryUser> Users { get; } = [
        new(Guid.NewGuid(), "Anna Weber", "anna.weber@contoso.de", "Finanzen", true, false, true),
        new(Guid.NewGuid(), "Lukas Müller", "lukas.mueller@contoso.de", "Vertrieb", true, false, false),
        new(Guid.NewGuid(), "Sofia Klein", "sofia.klein@contoso.de", "IT", true, false, true),
        new(Guid.NewGuid(), "Jonas Fischer", "jonas.fischer@contoso.de", "Logistik", true, true, true)
    ];
    public List<ServerStatus> Servers { get; } = [new("DC-01", "Domain Controller", "10.20.0.10", 23, 48, "Online"), new("FILE-01", "File Server", "10.20.0.20", 61, 72, "Warnung"), new("SQL-01", "SQL Server", "10.20.0.30", 34, 66, "Online")];
    public ConcurrentBag<AdminJob> Jobs { get; } = [];
}
