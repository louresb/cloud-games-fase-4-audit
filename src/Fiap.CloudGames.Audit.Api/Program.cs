using Fiap.CloudGames.Audit.Api.Middlewares;
using Fiap.CloudGames.Audit.Application;
using Fiap.CloudGames.Audit.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Serilog: Console + Loki, enrich with CorrelationId, TenantId
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Fiap.CloudGames.Audit.Api")
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(
        uri: builder.Configuration["Loki:Url"] ?? "http://localhost:3100",
        labels:
        [
            new LokiLabel { Key = "service", Value = "audit-svc" },
            new LokiLabel { Key = "env", Value = builder.Environment.EnvironmentName.ToLower() }
        ])
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddAuditApplication();
builder.Services.AddAuditInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<TenantMiddleware>();


app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

try
{
    Log.Information("Starting Fiap.CloudGames.Audit.Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

