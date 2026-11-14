namespace Content.Application.Features.Gallery.Queries.PreviewYoutubeVideo;

public sealed class PreviewYoutubeVideoValidator : AbstractValidator<PreviewYoutubeVideoQuery>
{
    public PreviewYoutubeVideoValidator() =>
        RuleFor(x => x.Url).Must(url => YoutubeVideoId.TryParse(url, out _))
            .WithMessage("Paste a YouTube video link, for example https://www.youtube.com/watch?v=… or https://youtu.be/….")
            .WithErrorCode("Content.Gallery.Youtube.Invalid").OverridePropertyName("url");
}
