namespace Content.Application.Features.Gallery.Admin.Delete;

public sealed class DeleteGalleryItemCommandValidator : AbstractValidator<DeleteGalleryItemCommand>
{
    public DeleteGalleryItemCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
