using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.AddVideo;

public sealed class AddGalleryVideoCommandValidator : AbstractValidator<AddGalleryVideoCommand>
{
    public AddGalleryVideoCommandValidator()
    {
        RuleFor(x => x.Url).Must(url => YoutubeVideoId.TryParse(url, out _))
            .WithMessage("Paste a YouTube video link, for example https://www.youtube.com/watch?v=… or https://youtu.be/….")
            .WithErrorCode("Content.Gallery.Youtube.Invalid");
        RuleFor(x => x.Texts!).SetValidator(new GalleryTextsInputValidator()).When(x => x.Texts is not null);
    }
}
