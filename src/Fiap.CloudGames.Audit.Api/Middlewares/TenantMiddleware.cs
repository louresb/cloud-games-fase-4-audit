using Fiap.CloudGames.Audit.Domain.Tenants;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Fiap.CloudGames.Audit.Api.Middlewares;

public sealed class TenantMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Tenant-Id";
    private const string LogPropertyName = "TenantId";

    public async Task InvokeAsync(HttpContext context)
    {
        var raw = context.Request.Headers.TryGetValue(HeaderName, out var value) ? value.ToString() : null;
        var tenant = Tenants.Normalize(raw);
        context.Items[HeaderName] = tenant;

        using (LogContext.PushProperty(LogPropertyName, tenant))
        {
            await next(context);
        }
    }
}
