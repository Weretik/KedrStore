using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
namespace Host.Api.Http;

/// <summary>
/// Gzip/Brotli for the offline-app snapshots only (spec platform/004, R-003). They are large, hold no secrets and
/// no reflected input, so compressing them over HTTPS is safe; other routes stay uncompressed.
/// </summary>
public static class MobileSyncCompressionRegistrationExtensions
{
    public const string MobileSyncPath = "/api/mobile/sync";

    public static IServiceCollection AddMobileSyncCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
        services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
        return services;
    }

    public static IApplicationBuilder UseMobileSyncCompression(this IApplicationBuilder app)
        => app.UseWhen(
            context => context.Request.Path.StartsWithSegments(MobileSyncPath, StringComparison.OrdinalIgnoreCase),
            branch => branch.UseResponseCompression());
}
