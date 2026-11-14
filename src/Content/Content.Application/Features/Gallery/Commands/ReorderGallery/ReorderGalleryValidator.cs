namespace Content.Application.Features.Gallery.Commands.ReorderGallery;

public sealed class ReorderGalleryValidator : AbstractValidator<ReorderGalleryCommand>
{
    public ReorderGalleryValidator()
    {
        RuleFor(x => x.Ids).NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Each card must appear once.");
        RuleForEach(x => x.Ids).GreaterThan(0);
    }
}
