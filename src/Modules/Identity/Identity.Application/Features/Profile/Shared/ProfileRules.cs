using FluentValidation;

namespace Identity.Application.Features.Profile.Shared;

public static class ProfileRules
{
    public const int FullNameMaxLength = 100;
    public const int EmailMaxLength = 256;

    public static IRuleBuilderOptions<T, string> FullName<T>(this IRuleBuilder<T, string> rule) =>
        rule.Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Вкажіть назву.")
            .Must(value => value is null || value.Trim().Length <= FullNameMaxLength)
            .WithMessage($"Ім’я — не більше {FullNameMaxLength} символів.");

    public static IRuleBuilderOptions<T, string> StaffEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().EmailAddress().MaximumLength(EmailMaxLength);
}
