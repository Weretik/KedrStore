namespace Content.Application.Features.Contacts.Commands.UpdateCompanyContacts;

/// <summary>Same rules as the domain, reported per field so the admin form can show them.</summary>
public sealed class UpdateCompanyContactsValidator : AbstractValidator<UpdateCompanyContactsCommand>
{
    private const string PhoneMessage = "Введіть український номер у форматі +380XXXXXXXXX.";

    public UpdateCompanyContactsValidator()
    {
        RuleFor(x => x.Input).NotNull().OverridePropertyName("body");
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.SalesPhone).Must(value => CompanyContacts.IsValidPhone(value?.Trim()))
                .WithMessage(PhoneMessage).OverridePropertyName("salesPhone");
            RuleFor(x => x.Input.ViberPhone).Must(value => CompanyContacts.IsValidPhone(value?.Trim()))
                .WithMessage(PhoneMessage).OverridePropertyName("viberPhone");
            RuleFor(x => x.Input.Email).NotEmpty().MaximumLength(CompanyContacts.EmailMaxLength)
                .Must(value => CompanyContacts.IsValidEmail(value?.Trim())).WithMessage("Введіть коректну електронну пошту.")
                .OverridePropertyName("email");
            RuleFor(x => x.Input.TelegramUrl).NotEmpty().MaximumLength(CompanyContacts.UrlMaxLength)
                .Must(value => CompanyContacts.IsValidTelegramUrl(value?.Trim()))
                .WithMessage("Введіть посилання https://t.me/….").OverridePropertyName("telegramUrl");
            RuleFor(x => x.Input.MapUrl).NotEmpty().MaximumLength(CompanyContacts.UrlMaxLength)
                .Must(value => CompanyContacts.IsValidHttpsUrl(value?.Trim()))
                .WithMessage("Введіть посилання, що починається з https://.").OverridePropertyName("mapUrl");
            RuleFor(x => x.Input.CityUk).NotEmpty().MaximumLength(CompanyContacts.CityMaxLength)
                .OverridePropertyName("cityUk");
            RuleFor(x => x.Input.CityRu).NotEmpty().MaximumLength(CompanyContacts.CityMaxLength)
                .OverridePropertyName("cityRu");
        });
    }
}
