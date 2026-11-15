using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Host.Api.Http.OpenApi;

/// <summary>
/// The OpenAPI document of the code. Every build writes it to <c>docs/sdd/contracts/openapi.json</c>, the only
/// machine-readable API contract (ADR-0013); Development also serves it at <c>/openapi/v1.json</c> with Swagger UI.
/// </summary>
public static class OpenApiRegistrationExtensions
{
    private const string DocumentUrl = "/openapi/v1.json";

    /// <summary>The build runs the host through <c>dotnet-getdocument</c> to write the document.</summary>
    private static bool IsBuildTimeDocumentRun =>
        Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

    public static IServiceCollection AddApiDocument(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer<StrictNumberSchemaTransformer>();
            options.AddSchemaTransformer<EnumSchemaTransformer>();
            options.AddOperationTransformer<CamelCaseQueryParameterTransformer>();
            options.AddDocumentTransformer<ResponsePropertiesRequiredTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        return services;
    }

    /// <summary>The build-time run starts the host without a database: it needs no background work such as the data
    /// protection key ring.</summary>
    public static IServiceCollection WithoutBackgroundWorkForBuildTimeDocument(this IServiceCollection services)
    {
        if (IsBuildTimeDocumentRun)
            services.RemoveAll<IHostedService>();

        return services;
    }

    public static WebApplication MapApiDocument(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();
        app.UseSwaggerUI(options => options.SwaggerEndpoint(DocumentUrl, "KedrStore API"));

        return app;
    }
}
