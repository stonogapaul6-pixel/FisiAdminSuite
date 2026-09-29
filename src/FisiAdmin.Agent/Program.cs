var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5080"));
builder.Services.AddHostedService<AgentWorker>();
await builder.Build().RunAsync();

sealed class AgentWorker(ILogger<AgentWorker> logger, IHttpClientFactory factory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await factory.CreateClient("api").GetAsync("/api/v1/health", stoppingToken);
                logger.LogInformation("FISI Agent Heartbeat: API {StatusCode}", response.StatusCode);
                // TODO: Nur signierte und erlaubte Jobs abrufen und über geprüfte Handler ausführen.
            }
            catch (Exception ex) { logger.LogWarning(ex, "API nicht erreichbar"); }
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
