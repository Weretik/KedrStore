namespace Content.Application.Features.Gallery.Admin.Reorder;

public sealed class ReorderGalleryCommandValidator : AbstractValidator<ReorderGalleryCommand>
{
    public ReorderGalleryCommandValidator()
    {
        RuleFor(x => x.Ids).NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Each card must appear once.");
        RuleForEach(x => x.Ids).GreaterThan(0);
    }
}
