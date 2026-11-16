using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BuildingBlocks.Observability;

public static class ObservabilityRegistrationExtensions
{
    /// <summary>
    /// Logs and traces of a web host (spec platform/008). Serilog reads levels and enrichers from the <c>Serilog</c>
    /// section and writes to the console: Cloud Logging JSON when a Google Cloud project is set, plain text otherwise.
    /// </summary>
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
                      ?? new ObservabilityOptions();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext();

        Log.Logger = (options.ExportsToGoogleCloud
                ? logger.WriteTo.Console(new CloudLoggingJsonFormatter(options.GoogleCloudProjectId!))
                : logger.WriteTo.Console())
            .CreateLogger();

        builder.Host.UseSerilog();
        CloudTracing.Add(builder.Services, options);

        return builder;
    }
}
