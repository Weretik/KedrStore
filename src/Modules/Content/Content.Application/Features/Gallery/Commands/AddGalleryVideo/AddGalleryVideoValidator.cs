using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.AddGalleryVideo;

public sealed class AddGalleryVideoValidator : AbstractValidator<AddGalleryVideoCommand>
{
    public AddGalleryVideoValidator()
    {
        RuleFor(x => x.Url).Must(url => YoutubeVideoId.TryParse(url, out _))
            .WithMessage("Вставте посилання на відео YouTube, наприклад https://www.youtube.com/watch?v=… або https://youtu.be/….")
            .WithErrorCode("Content.Gallery.Youtube.Invalid");
        RuleFor(x => x.Texts!).SetValidator(new GalleryTextsInputValidator()).When(x => x.Texts is not null);
    }
}
