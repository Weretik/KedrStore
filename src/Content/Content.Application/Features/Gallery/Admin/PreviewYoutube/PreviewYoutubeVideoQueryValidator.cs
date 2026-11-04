namespace Content.Application.Features.Gallery.Admin.PreviewYoutube;

public sealed class PreviewYoutubeVideoQueryValidator : AbstractValidator<PreviewYoutubeVideoQuery>
{
    public PreviewYoutubeVideoQueryValidator() =>
        RuleFor(x => x.Url).Must(url => YoutubeVideoId.TryParse(url, out _))
            .WithMessage("Paste a YouTube video link, for example https://www.youtube.com/watch?v=… or https://youtu.be/….")
            .WithErrorCode("Content.Gallery.Youtube.Invalid").OverridePropertyName("url");
}
