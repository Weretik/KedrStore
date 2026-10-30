using Catalog.Application.Slugs;

namespace Catalog.Application.Features.Slugs.Change;

public sealed class ChangeSlugCommandValidator : AbstractValidator<ChangeSlugCommand>
{
    public ChangeSlugCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Slug)
            .NotEmpty()
            .WithMessage("Slug is required unless regenerate is true.")
            .When(x => x.Request is { Regenerate: false });

        RuleFor(x => x.Request.Slug)
            .Must(slug => SlugText.IsValid(slug))
            .WithMessage($"Slug must use a-z, 0-9 and single hyphens, 1-{SlugText.MaxLength} characters.")
            .When(x => x.Request is { Regenerate: false } && !string.IsNullOrEmpty(x.Request.Slug))
            .OverridePropertyName("slug");
    }
}
