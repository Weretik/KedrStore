using System.Text.RegularExpressions;
using Content.Domain.Editing;

namespace Content.Domain.Contacts;

/// <summary>What the site shows as one set of company contacts.</summary>
public sealed record CompanyContactsValues(
    string SalesPhone,
    string Email,
    string TelegramUrl,
    string ViberPhone,
    string MapUrl,
    string CityUk,
    string CityRu);

/// <summary>
/// The company's public contacts: the sales phone, email, messengers, map, and city that every site page
/// shows. There is exactly one row (<see cref="SingletonId"/>); it is created by a migration and only updated.
/// </summary>
public sealed partial class CompanyContacts : IAggregateRoot
{
    public const int SingletonId = 1;
    public const int EmailMaxLength = 120;
    public const int UrlMaxLength = 500;
    public const int CityMaxLength = 80;

    public int Id { get; private set; }

    /// <summary>Ukrainian number in E.164, for example <c>+380681234567</c>; the site formats it.</summary>
    public string SalesPhone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string TelegramUrl { get; private set; } = null!;

    /// <summary>Number the Viber link opens, in E.164; often the sales phone.</summary>
    public string ViberPhone { get; private set; } = null!;
    public string MapUrl { get; private set; } = null!;
    public string CityUk { get; private set; } = null!;
    public string CityRu { get; private set; } = null!;
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Who saved the last change; empty for the row created by the migration.</summary>
    public string? UpdatedBy { get; private set; }

    private CompanyContacts() { }

    public static DomainResult<CompanyContacts> Create(CompanyContactsValues values, DateTimeOffset now)
    {
        var contacts = new CompanyContacts { Id = SingletonId };
        var applied = contacts.Apply(values, now);
        return applied.IsFailure ? applied.Error! : contacts;
    }

    public DomainResult Update(CompanyContactsValues values, DateTimeOffset now, string editor)
    {
        var applied = Apply(values, now);
        if (applied.IsFailure)
            return applied;
        UpdatedBy = EditorName.Normalize(editor);
        return DomainResult.Success();
    }

    private DomainResult Apply(CompanyContactsValues values, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(values);
        var checks = new DomainChecks();
        var salesPhone = checks.Take(Phone(values.SalesPhone, nameof(SalesPhone)));
        var email = checks.Take(Checked(values.Email, nameof(Email), EmailMaxLength, IsValidEmail));
        var telegramUrl = checks.Take(Checked(values.TelegramUrl, nameof(TelegramUrl), UrlMaxLength, IsValidTelegramUrl));
        var viberPhone = checks.Take(Phone(values.ViberPhone, nameof(ViberPhone)));
        var mapUrl = checks.Take(Checked(values.MapUrl, nameof(MapUrl), UrlMaxLength, IsValidHttpsUrl));
        var cityUk = checks.Take(Checked(values.CityUk, nameof(CityUk), CityMaxLength, _ => true));
        var cityRu = checks.Take(Checked(values.CityRu, nameof(CityRu), CityMaxLength, _ => true));
        if (checks.Error is { } error)
            return error;

        SalesPhone = salesPhone;
        Email = email;
        TelegramUrl = telegramUrl;
        ViberPhone = viberPhone;
        MapUrl = mapUrl;
        CityUk = cityUk;
        CityRu = cityRu;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    public static bool IsValidPhone(string? value) => value is not null && PhonePattern().IsMatch(value);

    public static bool IsValidEmail(string? value) => value is not null && EmailPattern().IsMatch(value);

    public static bool IsValidHttpsUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
           && !string.IsNullOrEmpty(uri.Host);

    public static bool IsValidTelegramUrl(string? value)
        => IsValidHttpsUrl(value) && new Uri(value!).Host is "t.me" or "telegram.me";

    private static DomainResult<string> Phone(string? value, string field)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return CompanyContactsErrors.Required(field);
        return IsValidPhone(trimmed) ? trimmed : CompanyContactsErrors.Invalid(field);
    }

    private static DomainResult<string> Checked(string? value, string field, int maxLength, Func<string, bool> isValid)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return CompanyContactsErrors.Required(field);
        if (trimmed.Length > maxLength)
            return CompanyContactsErrors.TooLong(field, maxLength, trimmed.Length);
        return isValid(trimmed) ? trimmed : CompanyContactsErrors.Invalid(field);
    }

    [GeneratedRegex(@"^\+380\d{9}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();
}
