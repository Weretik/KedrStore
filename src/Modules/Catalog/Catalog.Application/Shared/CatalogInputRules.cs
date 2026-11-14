namespace Catalog.Application.Shared;

/// <summary>Input rules shared by the public catalog queries: the content language and a looked-up slug.</summary>
public static class CatalogInputRules
{
    /// <summary>Longer than a new slug, so that a former or imported address still reaches its redirect.</summary>
    public const int LookupSlugMaxLength = 200;

    public static IRuleBuilderOptions<T, string?> SupportedLanguage<T>(this IRuleBuilder<T, string?> rule)
        => rule.Must(CatalogLanguage.IsSupported).WithMessage("Мова може бути лише uk або ru.");

    public static IRuleBuilderOptions<T, string> LookupSlug<T>(this IRuleBuilder<T, string> rule)
        => rule.NotEmpty().MaximumLength(LookupSlugMaxLength);
}
