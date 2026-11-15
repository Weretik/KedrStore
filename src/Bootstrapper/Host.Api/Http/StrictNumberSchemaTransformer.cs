using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Host.Api.Http;

/// <summary>
/// The web JSON defaults also read <c>"12"</c> as 12, so the generator documents every number as
/// <c>integer|string</c> with a pattern. The API answers and expects numbers, so the contract says only number.
/// </summary>
internal sealed class StrictNumberSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Type is { } type
            && type.HasFlag(JsonSchemaType.String)
            && (type.HasFlag(JsonSchemaType.Integer) || type.HasFlag(JsonSchemaType.Number)))
        {
            schema.Type = type & ~JsonSchemaType.String;
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}
