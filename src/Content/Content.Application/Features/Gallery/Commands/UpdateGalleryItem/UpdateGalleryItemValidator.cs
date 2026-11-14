using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.UpdateGalleryItem;

public sealed class UpdateGalleryItemValidator : AbstractValidator<UpdateGalleryItemCommand>
{
    public UpdateGalleryItemValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Texts).NotNull().SetValidator(new GalleryTextsInputValidator());
    }
}
