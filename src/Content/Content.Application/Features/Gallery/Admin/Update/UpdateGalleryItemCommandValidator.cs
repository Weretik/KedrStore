using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.Update;

public sealed class UpdateGalleryItemCommandValidator : AbstractValidator<UpdateGalleryItemCommand>
{
    public UpdateGalleryItemCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Texts).NotNull().SetValidator(new GalleryTextsInputValidator());
    }
}
