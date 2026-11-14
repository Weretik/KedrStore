namespace Content.Application.Features.Requisites.Commands.UpdateCompanyRequisites;

/// <summary>Same rules as the domain, reported per field so the admin form can show them.</summary>
public sealed class UpdateCompanyRequisitesValidator : AbstractValidator<UpdateCompanyRequisitesCommand>
{
    public UpdateCompanyRequisitesValidator()
    {
        RuleFor(x => x.Input).NotNull().OverridePropertyName("body");
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.LegalName).NotEmpty().MaximumLength(CompanyRequisites.LegalNameMaxLength)
                .OverridePropertyName("legalName");
            RuleFor(x => x.Input.Code).Must(value => CompanyRequisites.IsValidCode(value?.Trim()))
                .WithMessage("Введіть 8 цифр (ЄДРПОУ) або 10 цифр (РНОКПП).").OverridePropertyName("code");
            RuleFor(x => x.Input.RegistrationExtractNumber)
                .Must(value => CompanyRequisites.IsValidExtractNumber(value?.Trim()))
                .WithMessage($"Лише цифри, не більше {CompanyRequisites.ExtractNumberMaxLength}.")
                .OverridePropertyName("registrationExtractNumber");
            RuleFor(x => x.Input.Iban)
                .Must(value => CompanyRequisites.IsValidIban(CompanyRequisites.NormalizeIban(value)))
                .WithMessage("Введіть коректний український IBAN (UA і 27 цифр).").OverridePropertyName("iban");
            RuleFor(x => x.Input.BankName).NotEmpty().MaximumLength(CompanyRequisites.BankNameMaxLength)
                .OverridePropertyName("bankName");
            RuleFor(x => x.Input.BankCode).Must(value => CompanyRequisites.IsValidBankCode(value?.Trim()))
                .WithMessage("Введіть 6 цифр.").OverridePropertyName("bankCode");
            RuleFor(x => x.Input).Must(input => !CompanyRequisites.IsValidBankCode(input.BankCode?.Trim())
                    || !CompanyRequisites.IsValidIban(CompanyRequisites.NormalizeIban(input.Iban))
                    || CompanyRequisites.BankCodeMatchesIban(CompanyRequisites.NormalizeIban(input.Iban),
                        input.BankCode.Trim()))
                .WithMessage("Код банку має збігатися з 5–10 цифрами IBAN.").OverridePropertyName("bankCode");
        });
    }
}
