using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
namespace Host.Api.Network;

/// <summary>
/// Rate limits of the API (docs: security/rate-limiting.md). One soft global limit covers every request; named
/// policies, chosen on the endpoint with <c>RequireRateLimiting</c>, give a tighter limit where an endpoint is public,
/// expensive or credential-related. A request must pass both. The calling address is the real client address
/// restored by <c>TrustedProxyClientIpMiddleware</c>.
/// </summary>
public static class RateLimitingRegistrationExtensions
{
    public static IServiceCollection AddRateLimitingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var limits = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
                     ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(System.Globalization.CultureInfo.InvariantCulture);

                return ValueTask.CompletedTask;
            };

            // The soft limit for everything. Liveness and readiness probes and the scheduler tick are never limited.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                IsUnlimited(context.Request.Path)
                    ? RateLimitPartition.GetNoLimiter("unlimited")
                    : ByAddress(context, limits.GlobalPerMinute, TimeSpan.FromMinutes(1)));

            options.AddPolicy("AuthLogin", context => ByAddress(context, 10, TimeSpan.FromMinutes(1)));
            options.AddPolicy("AuthRefresh", context => ByAddress(context, 30, TimeSpan.FromMinutes(1)));

            // Anonymous quick orders: stops a bot from flooding the managers' Telegram chat. The public site sends
            // orders from its server, so all site buyers share one address; the cap is a flood limit, not per buyer.
            options.AddPolicy("QuickOrder", context => ByAddress(context, 60, TimeSpan.FromMinutes(10)));

            options.AddPolicy("PublicRead", context => ByAddress(context, limits.PublicReadPerMinute, TimeSpan.FromMinutes(1)));
            options.AddPolicy("PublicSearch", context => ByAddress(context, limits.PublicSearchPerMinute, TimeSpan.FromMinutes(1)));
            options.AddPolicy("Upload", context => ByAddress(context, limits.UploadPerMinute, TimeSpan.FromMinutes(1)));
            options.AddPolicy("HeavyOperation", context => ByAddress(context, limits.HeavyOperationPerMinute, TimeSpan.FromMinutes(1)));
        });

        return services;
    }

    private static bool IsUnlimited(PathString path) =>
        path.StartsWithSegments("/health") || path.StartsWithSegments("/internal");

    private static RateLimitPartition<string> ByAddress(HttpContext context, int permits, TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permits,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = true
            });
}
