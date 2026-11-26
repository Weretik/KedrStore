using Content.Application.Features.Contacts.Shared;
using Content.Domain.Contacts;
namespace Content.Api.Endpoints.Contacts.Shared;

/// <summary>
/// The shop customers visit (spec content/012 R-02): its Google Maps name, address in both languages, postal code, and
/// opening hours as <c>HH:mm</c>; a day without hours is a day off. Every field is optional.
/// </summary>
public sealed record CompanyStoreDto(
    string? StoreName = null,
    string? StreetAddressUk = null,
    string? StreetAddressRu = null,
    string? RegionUk = null,
    string? RegionRu = null,
    string? PostalCode = null,
    string? WeekdaysOpens = null,
    string? WeekdaysCloses = null,
    string? SaturdayOpens = null,
    string? SaturdayCloses = null,
    string? SundayOpens = null,
    string? SundayCloses = null);

/// <summary>Company contacts for the site and the admin. Phones are E.164 (<c>+380…</c>); the site formats them.</summary>
public sealed record CompanyContactsResponse(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu,
    CompanyStoreDto Store,
    DateTimeOffset UpdatedAt);

/// <summary>The contacts as the admin sees them, with who saved the last change (never shown on the site).</summary>
public sealed record AdminCompanyContactsResponse(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu,
    CompanyStoreDto Store,
    DateTimeOffset UpdatedAt,
    string? UpdatedBy);

/// <summary>All fields are replaced at once; without <c>Store</c> the shop's address and hours stay as they are.</summary>
public sealed record SaveCompanyContactsRequest(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu,
    CompanyStoreDto? Store = null);

internal static class CompanyContactsResponseMapper
{
    public static CompanyContactsResponse ToResponse(CompanyContactsDetails details) => new(
        details.SalesPhone, details.Email, details.TelegramUrl, details.ViberPhone, details.MapUrl,
        details.CityUk, details.CityRu, ToDto(details.Store), details.UpdatedAt);

    public static AdminCompanyContactsResponse ToAdminResponse(CompanyContactsDetails details) => new(
        details.SalesPhone, details.Email, details.TelegramUrl, details.ViberPhone, details.MapUrl,
        details.CityUk, details.CityRu, ToDto(details.Store), details.UpdatedAt, details.UpdatedBy);

    public static CompanyContactsInput ToInput(SaveCompanyContactsRequest request) => new(
        request.SalesPhone, request.Email, request.TelegramUrl, request.ViberPhone, request.MapUrl,
        request.CityUk, request.CityRu, request.Store is { } store ? ToValues(store) : null);

    private static CompanyStoreDto ToDto(CompanyStoreDetails store) => new(
        store.StoreName, store.StreetAddressUk, store.StreetAddressRu, store.RegionUk, store.RegionRu, store.PostalCode,
        store.WeekdaysOpens, store.WeekdaysCloses, store.SaturdayOpens, store.SaturdayCloses, store.SundayOpens,
        store.SundayCloses);

    private static CompanyStoreValues ToValues(CompanyStoreDto store) => new(
        store.StoreName, store.StreetAddressUk, store.StreetAddressRu, store.RegionUk, store.RegionRu, store.PostalCode,
        store.WeekdaysOpens, store.WeekdaysCloses, store.SaturdayOpens, store.SaturdayCloses, store.SundayOpens,
        store.SundayCloses);
}
