using System.Text.Json.Nodes;
using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Hosts;

/// <summary>
/// The build writes the OpenAPI document of the code to <c>docs/sdd/contracts/openapi.json</c> (ADR-0013); CI fails when
/// the committed file differs from it. These rules keep that generated contract usable by the typed frontend clients.
/// </summary>
public sealed class OpenApiDocumentRulesTests
{
    private static readonly string[] Methods = ["get", "post", "put", "patch", "delete"];

    [Fact]
    public void EveryOperation_HasAUniqueOperationId_ASummary_AndATag()
    {
        var operations = Operations().ToList();
        Assert.NotEmpty(operations);

        var violations = operations
            .Where(op => op.Operation["operationId"] is null || op.Operation["summary"] is null || op.Operation["tags"] is null)
            .Select(op => op.Name)
            .ToList();
        var duplicates = operations
            .Select(op => op.Operation["operationId"]?.GetValue<string>())
            .Where(id => id is not null)
            .GroupBy(id => id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key!);

        Assert.Empty(violations.Concat(duplicates));
    }

    [Fact]
    public void Numbers_AreNotAlsoStrings()
    {
        // System.Text.Json web defaults read "12" as 12, which the generator documents as integer|string with a
        // pattern; the contract promises numbers, so clients get number types.
        var violations = Nodes(Document())
            .OfType<JsonObject>()
            .Where(node => node["type"] is JsonArray types
                && types.Any(type => type?.GetValue<string>() == "string")
                && types.Any(type => type?.GetValue<string>() is "integer" or "number"))
            .Select(node => node.GetPath())
            .ToList();

        Assert.Empty(violations);
    }

    private static JsonNode Document() =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "docs", "sdd", "contracts", "openapi.json")))
        ?? throw new InvalidOperationException("docs/sdd/contracts/openapi.json is empty; build Host.Api.");

    private static IEnumerable<(string Name, JsonObject Operation)> Operations() =>
        (Document()["paths"] as JsonObject ?? [])
        .SelectMany(path => Methods
            .Where(method => path.Value?[method] is JsonObject)
            .Select(method => ($"{method.ToUpperInvariant()} {path.Key}", (JsonObject)path.Value![method]!)));

    private static IEnumerable<JsonNode> Nodes(JsonNode? node) => node switch
    {
        JsonObject obj => obj.SelectMany(property => Nodes(property.Value)).Prepend(obj),
        JsonArray array => array.SelectMany(Nodes).Prepend(array),
        null => [],
        _ => [node],
    };
}
