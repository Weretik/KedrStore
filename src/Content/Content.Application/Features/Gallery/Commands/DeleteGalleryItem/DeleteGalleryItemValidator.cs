namespace Content.Application.Features.Gallery.Commands.DeleteGalleryItem;

public sealed class DeleteGalleryItemValidator : AbstractValidator<DeleteGalleryItemCommand>
{
    public DeleteGalleryItemValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
