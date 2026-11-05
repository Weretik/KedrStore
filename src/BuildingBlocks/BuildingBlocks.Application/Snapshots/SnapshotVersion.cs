using System.Security.Cryptography;

namespace BuildingBlocks.Application.Snapshots;

/// <summary>
/// The version of a whole-data snapshot for an offline client: a hash of its JSON content. The same content always
/// gives the same version, any change gives a new one; the API sends it as the strong <c>ETag</c>.
/// </summary>
public static class SnapshotVersion
{
    public static string Compute<T>(T content)
    {
        var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
            content, System.Text.Json.JsonSerializerOptions.Web);
        return Convert.ToHexStringLower(SHA256.HashData(bytes).AsSpan(0, 16));
    }
}
