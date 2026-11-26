using System.Globalization;
using System.Text.RegularExpressions;

namespace Content.Domain.Contacts;

/// <summary>What the admin types for the shop: every field is optional; times are <c>HH:mm</c>.</summary>
public sealed record CompanyStoreValues(
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

/// <summary>
/// The shop customers can visit: its name as Google Maps shows it, the street address and region in both languages, the
/// postal code, and the opening hours (spec content/012 R-02). The site marks it up as <c>LocalBusiness</c> when the
/// street is filled in. A day without hours is a day off.
/// </summary>
public sealed partial class CompanyStore
{
    public const int NameMaxLength = 120;
    public const int StreetMaxLength = 200;
    public const int RegionMaxLength = 80;

    public string? StoreName { get; private set; }
    public string? StreetAddressUk { get; private set; }
    public string? StreetAddressRu { get; private set; }
    public string? RegionUk { get; private set; }
    public string? RegionRu { get; private set; }
    /// <summary>Five digits, for example <c>61038</c>.</summary>
    public string? PostalCode { get; private set; }
    public TimeOnly? WeekdaysOpens { get; private set; }
    public TimeOnly? WeekdaysCloses { get; private set; }
    public TimeOnly? SaturdayOpens { get; private set; }
    public TimeOnly? SaturdayCloses { get; private set; }
    public TimeOnly? SundayOpens { get; private set; }
    public TimeOnly? SundayCloses { get; private set; }

    private CompanyStore() { }

    public static CompanyStore Empty() => new();

    public static DomainResult<CompanyStore> Create(CompanyStoreValues values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var checks = new DomainChecks();
        var name = checks.Take(Text(values.StoreName, nameof(StoreName), NameMaxLength));
        var streetUk = checks.Take(Text(values.StreetAddressUk, nameof(StreetAddressUk), StreetMaxLength));
        var streetRu = checks.Take(Text(values.StreetAddressRu, nameof(StreetAddressRu), StreetMaxLength));
        var regionUk = checks.Take(Text(values.RegionUk, nameof(RegionUk), RegionMaxLength));
        var regionRu = checks.Take(Text(values.RegionRu, nameof(RegionRu), RegionMaxLength));
        var postalCode = checks.Take(Postal(values.PostalCode));
        var weekdays = checks.Take(Hours(values.WeekdaysOpens, values.WeekdaysCloses, "Weekdays"));
        var saturday = checks.Take(Hours(values.SaturdayOpens, values.SaturdayCloses, "Saturday"));
        var sunday = checks.Take(Hours(values.SundayOpens, values.SundayCloses, "Sunday"));
        if (checks.Error is { } error)
            return error;

        return new CompanyStore
        {
            StoreName = name, StreetAddressUk = streetUk, StreetAddressRu = streetRu, RegionUk = regionUk,
            RegionRu = regionRu, PostalCode = postalCode, WeekdaysOpens = weekdays.Opens, WeekdaysCloses = weekdays.Closes,
            SaturdayOpens = saturday.Opens, SaturdayCloses = saturday.Closes, SundayOpens = sunday.Opens,
            SundayCloses = sunday.Closes
        };
    }

    /// <summary>A time typed as <c>H:mm</c> or <c>HH:mm</c>; <c>null</c> when it is not one.</summary>
    public static TimeOnly? ParseTime(string? value)
        => TimeOnly.TryParseExact(value?.Trim(), ["H:mm", "HH:mm"], CultureInfo.InvariantCulture, DateTimeStyles.None,
            out var time) ? time : null;

    public static bool IsValidPostalCode(string? value) => value is null || PostalPattern().IsMatch(value.Trim());

    private static DomainResult<string?> Text(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (string?)null;
        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? CompanyContactsErrors.TooLong(field, maxLength, trimmed.Length) : trimmed;
    }

    private static DomainResult<string?> Postal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (string?)null;
        return IsValidPostalCode(value) ? value.Trim() : CompanyContactsErrors.Invalid(nameof(PostalCode));
    }

    /// <summary>Both times or neither; the shop opens before it closes.</summary>
    private static DomainResult<(TimeOnly? Opens, TimeOnly? Closes)> Hours(string? opens, string? closes, string day)
    {
        if (string.IsNullOrWhiteSpace(opens) && string.IsNullOrWhiteSpace(closes))
            return ((TimeOnly?)null, (TimeOnly?)null);
        if (ParseTime(opens) is not { } from)
            return CompanyContactsErrors.Invalid(day + "Opens");
        if (ParseTime(closes) is not { } to || to <= from)
            return CompanyContactsErrors.Invalid(day + "Closes");
        return ((TimeOnly?)from, (TimeOnly?)to);
    }

    [GeneratedRegex(@"^\d{5}$", RegexOptions.CultureInvariant)]
    private static partial Regex PostalPattern();
}
