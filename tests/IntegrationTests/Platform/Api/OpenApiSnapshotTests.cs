using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Platform.Api;

/// <summary>
/// Golden OpenAPI document (platform spec 005, R-007): moving controllers to Minimal API must not change
/// paths, parameters, schemas, tags or status codes. Both documents are compared after normalization: sorted keys,
/// tags in name order, and only the JSON media type of each body (MVC also lists <c>text/json</c>, <c>text/plain</c> and
/// <c>application/*+json</c>; Minimal API errors are <c>application/problem+json</c>). Set
/// UPDATE_OPENAPI_SNAPSHOT=1 to rewrite the snapshot after an intended contract change.
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

    private static readonly string[] ExtraMediaTypes = ["text/json", "text/plain", "application/*+json"];

    private static string Normalize(string json)
    {
        var node = JsonNode.Parse(json) ?? throw new InvalidOperationException("OpenAPI document is empty.");
        foreach (var content in Contents(node))
        {
            foreach (var mediaType in ExtraMediaTypes)
                content.Remove(mediaType);
            if (content["application/problem+json"] is { } problem && content["application/json"] is null)
            {
                content.Remove("application/problem+json");
                content["application/json"] = problem;
            }
        }

        // The tag list follows endpoint registration order, which says nothing about the contract.
        if (node["tags"] is JsonArray tags)
            node["tags"] = new JsonArray(tags
                .OrderBy(tag => tag?["name"]?.GetValue<string>(), StringComparer.Ordinal)
                .Select(tag => tag?.DeepClone())
                .ToArray());

        return Sort(node)!.ToJsonString(Indented).ReplaceLineEndings("\n") + "\n";
    }

    /// <summary>The <c>content</c> maps of every request body and response of every operation.</summary>
    private static IEnumerable<JsonObject> Contents(JsonNode document)
    {
        if (document["paths"] is not JsonObject paths)
            yield break;
        foreach (var operation in paths.Select(path => path.Value).OfType<JsonObject>()
                     .SelectMany(path => path.Select(method => method.Value).OfType<JsonObject>()))
        {
            if (operation["requestBody"]?["content"] is JsonObject requestContent)
                yield return requestContent;
            if (operation["responses"] is not JsonObject responses)
                continue;
            foreach (var response in responses.Select(response => response.Value).OfType<JsonObject>())
                if (response["content"] is JsonObject responseContent)
                    yield return responseContent;
        }
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
