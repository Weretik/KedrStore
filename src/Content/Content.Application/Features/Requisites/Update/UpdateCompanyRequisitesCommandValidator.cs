namespace Content.Application.Features.Requisites.Update;

/// <summary>Same rules as the domain, reported per field so the admin form can show them.</summary>
public sealed class UpdateCompanyRequisitesCommandValidator : AbstractValidator<UpdateCompanyRequisitesCommand>
{
    public UpdateCompanyRequisitesCommandValidator()
    {
        RuleFor(x => x.Input).NotNull().OverridePropertyName("body");
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.LegalName).NotEmpty().MaximumLength(CompanyRequisites.LegalNameMaxLength)
                .OverridePropertyName("legalName");
            RuleFor(x => x.Input.Code).Must(value => CompanyRequisites.IsValidCode(value?.Trim()))
                .WithMessage("Use 8 digits (ЄДРПОУ) or 10 digits (РНОКПП).").OverridePropertyName("code");
            RuleFor(x => x.Input.RegistrationExtractNumber)
                .Must(value => CompanyRequisites.IsValidExtractNumber(value?.Trim()))
                .WithMessage($"Use digits only, at most {CompanyRequisites.ExtractNumberMaxLength}.")
                .OverridePropertyName("registrationExtractNumber");
            RuleFor(x => x.Input.Iban)
                .Must(value => CompanyRequisites.IsValidIban(CompanyRequisites.NormalizeIban(value)))
                .WithMessage("Enter a valid Ukrainian IBAN (UA and 27 digits).").OverridePropertyName("iban");
            RuleFor(x => x.Input.BankName).NotEmpty().MaximumLength(CompanyRequisites.BankNameMaxLength)
                .OverridePropertyName("bankName");
            RuleFor(x => x.Input.BankCode).Must(value => CompanyRequisites.IsValidBankCode(value?.Trim()))
                .WithMessage("Use 6 digits.").OverridePropertyName("bankCode");
            RuleFor(x => x.Input).Must(input => !CompanyRequisites.IsValidBankCode(input.BankCode?.Trim())
                    || !CompanyRequisites.IsValidIban(CompanyRequisites.NormalizeIban(input.Iban))
                    || CompanyRequisites.BankCodeMatchesIban(CompanyRequisites.NormalizeIban(input.Iban),
                        input.BankCode.Trim()))
                .WithMessage("The bank code must match digits 5-10 of the IBAN.").OverridePropertyName("bankCode");
        });
    }
}
