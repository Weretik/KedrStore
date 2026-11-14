namespace Content.Application.Features.Gallery.Commands.AddGalleryPhotos;

public sealed class AddGalleryPhotosValidator : AbstractValidator<AddGalleryPhotosCommand>
{
    public const int MaxFiles = 100;

    public AddGalleryPhotosValidator()
    {
        RuleFor(x => x.MediaFileIds).NotEmpty().Must(ids => ids.Count <= MaxFiles)
            .WithMessage($"Add at most {MaxFiles} photos at once.");
        RuleForEach(x => x.MediaFileIds).GreaterThan(0);
    }
}
