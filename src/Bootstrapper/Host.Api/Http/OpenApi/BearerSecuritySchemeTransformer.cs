using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Host.Api.Http.OpenApi;

/// <summary>
/// Documents the mobile bearer token. Browsers sign in with the session cookie instead (see the frontend integration
/// guide); endpoint access stays with the authorization policies, not with this document.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeName = "bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "Opaque",
            Description = "The accessToken returned by POST /api/auth/mobile/login.",
        };

        return Task.CompletedTask;
    }
}
