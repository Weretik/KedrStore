using System.Globalization;

namespace Content.Application.Features.Contacts.Shared;

/// <summary>Admin input and the stored contacts share one shape.</summary>
public sealed record CompanyContactsInput(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu,
    CompanyStoreValues? Store = null);

/// <summary>The shop's address and hours as clients see them; times are <c>HH:mm</c>, <c>null</c> is a day off.</summary>
public sealed record CompanyStoreDetails(
    string? StoreName,
    string? StreetAddressUk,
    string? StreetAddressRu,
    string? RegionUk,
    string? RegionRu,
    string? PostalCode,
    string? WeekdaysOpens,
    string? WeekdaysCloses,
    string? SaturdayOpens,
    string? SaturdayCloses,
    string? SundayOpens,
    string? SundayCloses);

public sealed record CompanyContactsDetails(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu,
    CompanyStoreDetails Store,
    DateTimeOffset UpdatedAt,
    string? UpdatedBy);

internal static class CompanyContactsMapper
{
    public static CompanyContactsDetails ToDetails(CompanyContacts contacts) => new(
        contacts.SalesPhone, contacts.Email, contacts.TelegramUrl, contacts.ViberPhone, contacts.MapUrl,
        contacts.CityUk, contacts.CityRu, ToDetails(contacts.Store), contacts.UpdatedAt, contacts.UpdatedBy);

    public static CompanyContactsValues ToValues(CompanyContactsInput input) => new(
        input.SalesPhone, input.Email, input.TelegramUrl, input.ViberPhone, input.MapUrl,
        input.CityUk, input.CityRu, input.Store);

    private static CompanyStoreDetails ToDetails(CompanyStore store) => new(
        store.StoreName, store.StreetAddressUk, store.StreetAddressRu, store.RegionUk, store.RegionRu, store.PostalCode,
        Time(store.WeekdaysOpens), Time(store.WeekdaysCloses), Time(store.SaturdayOpens), Time(store.SaturdayCloses),
        Time(store.SundayOpens), Time(store.SundayCloses));

    private static string? Time(TimeOnly? time) => time?.ToString("HH:mm", CultureInfo.InvariantCulture);
}
