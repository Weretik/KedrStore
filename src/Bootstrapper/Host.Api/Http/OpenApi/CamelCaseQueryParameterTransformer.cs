using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Host.Api.Http.OpenApi;

/// <summary>
/// Query parameters bound with <c>[AsParameters]</c> keep the C# property name (<c>SearchTerm</c>). The server reads
/// the query string case-insensitively and the clients send camelCase, so the contract names them in camelCase.
/// </summary>
internal sealed class CamelCaseQueryParameterTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var parameter in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
        {
            if (parameter.In == ParameterLocation.Query && parameter.Name is { Length: > 0 } name && char.IsUpper(name[0]))
                parameter.Name = JsonNamingPolicy.CamelCase.ConvertName(name);
        }

        return Task.CompletedTask;
    }
}
