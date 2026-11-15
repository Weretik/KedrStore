using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Host.Api.Http.OpenApi;

/// <summary>
/// The generator documents an enum without a string converter as a bare <c>integer</c>, so clients do not know its
/// values. A query parameter binds the member name (<c>sort=IdAsc</c>), so there it is a string of the names; in a
/// JSON body it is the number, so there it lists the numbers.
/// </summary>
internal sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;
        if (!type.IsEnum || schema.Enum is { Count: > 0 })
            return Task.CompletedTask;

        if (context.ParameterDescription is not null)
        {
            schema.Type = JsonSchemaType.String;
            schema.Enum = Enum.GetNames(type).Select(name => (JsonNode)JsonValue.Create(name)).ToList();
        }
        else
        {
            schema.Enum = Enum.GetValuesAsUnderlyingType(type).Cast<object>()
                .Select(value => (JsonNode)JsonValue.Create(Convert.ToInt64(value))).ToList();
        }

        return Task.CompletedTask;
    }
}
