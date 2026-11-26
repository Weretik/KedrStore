namespace Content.Application.Features.Redirects.Shared;

/// <summary>Input limits of both addresses; the address rules themselves live in <see cref="SiteRedirectPath"/>.</summary>
internal static class SiteRedirectInputRules
{
    public static void AddressRules<T>(this AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, string?>> from,
        System.Linq.Expressions.Expression<Func<T, string?>> to)
    {
        validator.RuleFor(from).NotEmpty().WithMessage("Вкажіть стару адресу.")
            .MaximumLength(SiteRedirectPath.ToMaxLength).WithMessage($"Не більше {SiteRedirectPath.ToMaxLength} символів.")
            .OverridePropertyName("from");
        validator.RuleFor(to).NotEmpty().WithMessage("Вкажіть нову адресу.")
            .MaximumLength(SiteRedirectPath.ToMaxLength).WithMessage($"Не більше {SiteRedirectPath.ToMaxLength} символів.")
            .OverridePropertyName("to");
    }
}
