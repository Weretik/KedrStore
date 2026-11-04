namespace Media.Application.Jobs.CdnSync;

/// <summary>Picture types the sync job registers, by file extension (it does not download files).</summary>
internal static class MediaContentTypes
{
    private static readonly Dictionary<string, string> ByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif"
    };

    public static string? FromKey(string key) => ByExtension.GetValueOrDefault(Path.GetExtension(key));
}
