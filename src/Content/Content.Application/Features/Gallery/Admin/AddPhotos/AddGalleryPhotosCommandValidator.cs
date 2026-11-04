namespace Content.Application.Features.Gallery.Admin.AddPhotos;

public sealed class AddGalleryPhotosCommandValidator : AbstractValidator<AddGalleryPhotosCommand>
{
    public const int MaxFiles = 100;

    public AddGalleryPhotosCommandValidator()
    {
        RuleFor(x => x.MediaFileIds).NotEmpty().Must(ids => ids.Count <= MaxFiles)
            .WithMessage($"Add at most {MaxFiles} photos at once.");
        RuleForEach(x => x.MediaFileIds).GreaterThan(0);
    }
}
