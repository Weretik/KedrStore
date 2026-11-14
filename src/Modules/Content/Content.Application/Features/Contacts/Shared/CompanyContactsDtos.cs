namespace Content.Application.Features.Contacts.Shared;

/// <summary>Admin input and the stored contacts share one shape.</summary>
public sealed record CompanyContactsInput(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu);

public sealed record CompanyContactsDetails(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu,
    DateTimeOffset UpdatedAt,
    string? UpdatedBy);

internal static class CompanyContactsMapper
{
    public static CompanyContactsDetails ToDetails(CompanyContacts contacts) => new(
        contacts.SalesPhone, contacts.Email, contacts.TelegramUrl, contacts.ViberPhone, contacts.MapUrl,
        contacts.CityUk, contacts.CityRu, contacts.UpdatedAt, contacts.UpdatedBy);

    public static CompanyContactsValues ToValues(CompanyContactsInput input) => new(
        input.SalesPhone, input.Email, input.TelegramUrl, input.ViberPhone, input.MapUrl,
        input.CityUk, input.CityRu);
}
