namespace Content.Application.Features.Gallery.Commands.ReorderGallery;

public sealed class ReorderGalleryValidator : AbstractValidator<ReorderGalleryCommand>
{
    public ReorderGalleryValidator()
    {
        RuleFor(x => x.Ids).NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Кожна картка має бути лише один раз.");
        RuleForEach(x => x.Ids).GreaterThan(0);
    }
}
