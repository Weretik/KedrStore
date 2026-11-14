using Catalog.Application.Features.Slugs.Shared;

namespace Catalog.Application.Features.Slugs.Commands.ChangeSlug;

public sealed class ChangeSlugValidator : AbstractValidator<ChangeSlugCommand>
{
    public ChangeSlugValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Slug)
            .NotEmpty()
            .WithMessage("Вкажіть slug або увімкніть його автоматичне створення.")
            .When(x => x.Request is { Regenerate: false });

        RuleFor(x => x.Request.Slug)
            .Must(slug => SlugText.IsValid(slug))
            .WithMessage($"Slug: латинські літери a-z, цифри та одинарні дефіси, від 1 до {SlugText.MaxLength} символів.")
            .When(x => x.Request is { Regenerate: false } && !string.IsNullOrEmpty(x.Request.Slug))
            .OverridePropertyName("slug");
    }
}
