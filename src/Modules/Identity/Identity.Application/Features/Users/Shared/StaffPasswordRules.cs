using FluentValidation;

namespace Identity.Application.Features.Users.Shared;

public static class StaffPasswordRules
{
    // Staff passwords are stricter than the Identity minimum, which imported customer passwords still need.
    public const int MinimumLength = 8;

    public static IRuleBuilderOptions<T, string> StaffPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MinimumLength(MinimumLength).MaximumLength(128);
}
