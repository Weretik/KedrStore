namespace Content.Application.Features.Gallery.Shared;

/// <summary>YouTube's own poster for a video; <c>hqdefault</c> exists for every public video.</summary>
public static class YoutubeThumbnails
{
    public const int Width = 480;
    public const int Height = 360;

    public static string Url(string youtubeId) => $"https://i.ytimg.com/vi/{youtubeId}/hqdefault.jpg";
}
