using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Host.Api.Http.OpenApi;

/// <summary>
/// The API writes every property of a response (no property is skipped when null), but the generator marks only
/// constructor parameters as required, so <c>init</c> properties looked optional to clients. Every object reachable
/// from a response lists all its properties as required; nullable ones stay nullable.
/// </summary>
internal sealed class ResponsePropertiesRequiredTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var responseSchemas = Of(document.Paths?.Values)
            .SelectMany(path => Of(path.Operations?.Values))
            .SelectMany(operation => Of(operation.Responses?.Values))
            .SelectMany(response => Of(response.Content?.Values))
            .Select(media => media.Schema);

        foreach (var schema in responseSchemas)
            Visit(schema, document, visited);

        return Task.CompletedTask;
    }

    private static IEnumerable<T> Of<T>(IEnumerable<T>? items) => items ?? [];

    private static void Visit(IOpenApiSchema? schema, OpenApiDocument document, HashSet<string> visited)
    {
        if (schema is OpenApiSchemaReference reference)
        {
            var id = reference.Reference.Id;
            if (id is null || !visited.Add(id) || document.Components?.Schemas?.TryGetValue(id, out var target) != true)
                return;
            schema = target;
        }

        if (schema is not OpenApiSchema concrete)
            return;

        if (concrete.Properties is { Count: > 0 } properties)
        {
            concrete.Required ??= new HashSet<string>(StringComparer.Ordinal);
            foreach (var (name, property) in properties)
            {
                concrete.Required.Add(name);
                Visit(property, document, visited);
            }
        }

        Visit(concrete.Items, document, visited);
        Visit(concrete.AdditionalProperties, document, visited);
        foreach (var part in Of(concrete.AllOf).Concat(Of(concrete.AnyOf)).Concat(Of(concrete.OneOf)))
            Visit(part, document, visited);
    }
}
