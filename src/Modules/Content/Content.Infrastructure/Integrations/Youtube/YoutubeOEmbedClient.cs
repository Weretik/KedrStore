using System.Net.Http.Json;
using Content.Application.Abstractions.Integrations;
using Microsoft.Extensions.Logging;

namespace Content.Infrastructure.Integrations.Youtube;

/// <summary>
/// Reads a video title from YouTube's public oEmbed endpoint (no key). Any failure, including a private or deleted
/// video, gives <c>null</c>: the title is only a convenience and the manager can type it.
/// </summary>
internal sealed class YoutubeOEmbedClient(HttpClient http, ILogger<YoutubeOEmbedClient> logger) : IYoutubeVideoInfo
{
    public async Task<string?> GetTitleAsync(string youtubeId, CancellationToken cancellationToken)
    {
        var videoUrl = Uri.EscapeDataString($"https://www.youtube.com/watch?v={youtubeId}");
        try
        {
            using var response = await http.GetAsync($"https://www.youtube.com/oembed?format=json&url={videoUrl}",
                cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;
            var body = await response.Content.ReadFromJsonAsync<OEmbedResponse>(cancellationToken);
            var title = body?.Title?.Trim();
            return string.IsNullOrEmpty(title) ? null : title[..Math.Min(title.Length, 150)];
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
                                              or System.Text.Json.JsonException)
        {
            logger.LogInformation(exception, "YouTube oEmbed did not answer for {YoutubeId}", youtubeId);
            return null;
        }
    }

    private sealed record OEmbedResponse(string? Title);
}
