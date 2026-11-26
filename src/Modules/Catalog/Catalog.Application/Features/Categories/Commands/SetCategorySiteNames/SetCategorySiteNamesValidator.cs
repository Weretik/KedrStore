namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteNames;

public sealed class SetCategorySiteNamesValidator : AbstractValidator<SetCategorySiteNamesCommand>
{
    public SetCategorySiteNamesValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.NameUk).Must(BeShortEnough)
            .WithMessage($"Не більше {ProductCategory.ShortNameMaxLength} символів.").OverridePropertyName("nameUk");
        RuleFor(command => command.NameRu).Must(BeShortEnough)
            .WithMessage($"Не більше {ProductCategory.ShortNameMaxLength} символів.").OverridePropertyName("nameRu");
    }

    private static bool BeShortEnough(string? name)
        => name is null || name.Trim().Length <= ProductCategory.ShortNameMaxLength;
}
