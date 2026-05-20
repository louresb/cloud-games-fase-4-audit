using Fiap.CloudGames.Audit.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.CloudGames.Audit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}
