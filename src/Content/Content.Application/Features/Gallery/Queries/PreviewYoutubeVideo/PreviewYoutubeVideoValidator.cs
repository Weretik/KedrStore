namespace Content.Application.Features.Gallery.Queries.PreviewYoutubeVideo;

public sealed class PreviewYoutubeVideoValidator : AbstractValidator<PreviewYoutubeVideoQuery>
{
    public PreviewYoutubeVideoValidator() =>
        RuleFor(x => x.Url).Must(url => YoutubeVideoId.TryParse(url, out _))
            .WithMessage("Вставте посилання на відео YouTube, наприклад https://www.youtube.com/watch?v=… або https://youtu.be/….")
            .WithErrorCode("Content.Gallery.Youtube.Invalid").OverridePropertyName("url");
}
