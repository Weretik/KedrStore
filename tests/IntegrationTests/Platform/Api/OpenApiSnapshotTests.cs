using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Platform.Api;

/// <summary>
/// Golden OpenAPI document (platform spec 005, R-007): moving controllers to Minimal API must not change
/// paths, parameters, schemas, tags or status codes. Set UPDATE_OPENAPI_SNAPSHOT=1 to rewrite the snapshot
/// after an intended contract change.
/// </summary>
public sealed class OpenApiSnapshotTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    [Fact]
    public async Task OpenApi_Document_Matches_Committed_Snapshot()
    {
        using var client = factory.CreateClient();
        var json = await client.GetStringAsync("/openapi/v1.json");
        var actual = Normalize(json);
        var snapshotPath = SnapshotPath();

        if (Environment.GetEnvironmentVariable("UPDATE_OPENAPI_SNAPSHOT") == "1")
        {
            await File.WriteAllTextAsync(snapshotPath, actual);
            return;
        }

        Assert.True(File.Exists(snapshotPath), $"Snapshot {snapshotPath} is missing; run with UPDATE_OPENAPI_SNAPSHOT=1.");
        var expected = Normalize(await File.ReadAllTextAsync(snapshotPath));
        Assert.Equal(expected, actual);
    }

    private static string Normalize(string json)
    {
        var node = JsonNode.Parse(json) ?? throw new InvalidOperationException("OpenAPI document is empty.");
        return Sort(node)!.ToJsonString(Indented).ReplaceLineEndings("\n") + "\n";
    }

    private static JsonNode? Sort(JsonNode? node) => node switch
    {
        JsonObject obj => new JsonObject(obj
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => KeyValuePair.Create(p.Key, Sort(p.Value)))),
        JsonArray array => new JsonArray(array.Select(Sort).ToArray()),
        _ => node?.DeepClone(),
    };

    private static string SnapshotPath([CallerFilePath] string sourceFile = "") =>
        Path.Combine(Path.GetDirectoryName(sourceFile)!, "openapi.v1.json");
}
