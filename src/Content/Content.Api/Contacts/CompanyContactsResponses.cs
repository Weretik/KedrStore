using Content.Application.Features.Contacts.Common;

namespace Content.Api.Contacts;

/// <summary>Company contacts for the site and the admin. Phones are E.164 (<c>+380…</c>); the site formats them.</summary>
public sealed record CompanyContactsResponse(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu,
    DateTimeOffset UpdatedAt);

/// <summary>All fields are replaced at once.</summary>
public sealed record SaveCompanyContactsRequest(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu);

internal static class CompanyContactsResponseMapper
{
    public static CompanyContactsResponse ToResponse(CompanyContactsDetails details) => new(
        details.SalesPhone, details.Email, details.TelegramUrl, details.ViberPhone, details.MapUrl,
        details.CityUk, details.CityRu, details.UpdatedAt);

    public static CompanyContactsInput ToInput(SaveCompanyContactsRequest request) => new(
        request.SalesPhone, request.Email, request.TelegramUrl, request.ViberPhone, request.MapUrl,
        request.CityUk, request.CityRu);
}
