namespace Content.Application.Abstractions.Integrations;

/// <summary>Public facts about a YouTube video, used to prefill the title; <c>null</c> when YouTube does not answer.</summary>
public interface IYoutubeVideoInfo
{
    Task<string?> GetTitleAsync(string youtubeId, CancellationToken cancellationToken);
}
