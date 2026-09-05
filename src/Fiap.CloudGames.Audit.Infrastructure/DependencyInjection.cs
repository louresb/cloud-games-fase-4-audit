using System.Reflection;
using Amazon.DynamoDBv2;
using Fiap.CloudGames.Audit.Application.Consumers;
using Fiap.CloudGames.Audit.Domain.Repositories;
using Fiap.CloudGames.Audit.Infrastructure.Persistence.DynamoDb;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.CloudGames.Audit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new DynamoDbAuditOptions
        {
            TableName = configuration["DynamoDb:TableName"] ?? "cloud-games-audit-events",
            ServiceUrl = configuration["DynamoDb:ServiceUrl"],
            Region = configuration["DynamoDb:Region"] ?? configuration["AWS:Region"] ?? "us-east-1",
            AutoCreateTable = bool.TryParse(configuration["DynamoDb:AutoCreateTable"], out var auto) ? auto : true
        };

        services.AddSingleton(options);

        services.AddSingleton<IAmazonDynamoDB>(_ =>
        {
            var cfg = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(options.Region)
            };

            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                cfg.ServiceURL = options.ServiceUrl;
                cfg.UseHttp = options.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
                cfg.AuthenticationRegion = options.Region;
            }

            return new AmazonDynamoDBClient(cfg);
        });

        services.AddSingleton<IAuditRepository, DynamoDbAuditRepository>();
        services.AddHostedService<DynamoDbTableInitializer>();

        AddMessaging(services, configuration);

        return services;
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var auditQueue = configuration["Queues:Audit:Events"] ?? "audit.events";
        var consumerAssembly = typeof(AuditConsumerBase<>).Assembly;

        // MassTransit + RabbitMQ transport used by the current deployment topology.
        services.AddMassTransit(x =>
        {
            x.AddConsumers(consumerAssembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:HostName"] ?? "localhost";
                var user = configuration["RabbitMq:UserName"] ?? "guest";
                var pass = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.ConnectionName("Fiap.CloudGames.Audit.Api");
                    h.Username(user);
                    h.Password(pass);
                });

                cfg.ReceiveEndpoint(auditQueue, e =>
                {
                    ConfigureAllConsumers(e, context, consumerAssembly);
                });
            });
        });
    }

    private static void ConfigureAllConsumers(
        IReceiveEndpointConfigurator endpoint,
        IBusRegistrationContext context,
        Assembly consumerAssembly)
    {
        var consumerTypes = consumerAssembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsClass && typeof(IConsumer).IsAssignableFrom(t));

        foreach (var consumerType in consumerTypes)
        {
            endpoint.ConfigureConsumer(context, consumerType);
        }
    }
}
