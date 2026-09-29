namespace FisiAdmin.Domain;

public static class AppRoles
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string AdAdmin = "AdAdmin";
    public const string ServiceDesk = "ServiceDesk";
    public const string Auditor = "Auditor";
    public const string ReadOnly = "ReadOnly";
}

public sealed record LoginRequest(string UserName, string Password);
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, string DisplayName, string[] Roles);
public sealed record CurrentUser(string UserName, string DisplayName, string[] Roles);
public sealed record DirectoryUser(Guid Id, string DisplayName, string UserPrincipalName, string Department, bool Enabled, bool Locked, bool MfaEnabled);
public sealed record ServerStatus(string Name, string Role, string IpAddress, int CpuPercent, int RamPercent, string State);
public sealed record DashboardSummary(int ServersOnline, int ServersTotal, int ActiveUsers, int OpenJobs, int SecurityScore);
public sealed record CreateUserRequest(string FirstName, string LastName, string UserPrincipalName, string Department, string OrganizationalUnit, string[] Groups, bool CreateHomeDirectory);
public sealed record AdminJob(Guid Id, string Type, string Target, string Status, DateTimeOffset CreatedAt, string RequestedBy, string? Message = null);
