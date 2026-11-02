public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantRepository tenantRepository,
        ITenantService tenantService,
        IConfiguration config)
    {
        var tenantType = config["TenantType"];
        if (string.Equals(tenantType, "single", StringComparison.OrdinalIgnoreCase))
        {
            var conn = config.GetConnectionString("Master");
            var tenantCode = context.User?.FindFirst("tenant")?.Value ?? "default";
            tenantService.SetTenant(tenantCode, conn ?? string.Empty);
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        if (path.StartsWith("/api/admin"))
        {
            await _next(context);
            return;
        }

        var tenantCodeFromToken = context.User?.FindFirst("tenant")?.Value;

        if (!string.IsNullOrEmpty(tenantCodeFromToken))
        {
            var conn = await tenantRepository.GetConnectionStringAsync(tenantCodeFromToken);

            if (!string.IsNullOrEmpty(conn))
            {
                // ? SAME scoped instance used everywhere
                tenantService.SetTenant(tenantCodeFromToken, conn);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid tenant.");
                return;
            }
        }

        await _next(context);
    }
}
