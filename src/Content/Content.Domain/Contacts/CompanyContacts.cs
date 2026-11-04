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

    public static CompanyContacts Create(CompanyContactsValues values, DateTimeOffset now)
    {
        var contacts = new CompanyContacts { Id = SingletonId };
        contacts.Apply(values, now);
        return contacts;
    }

    public void Update(CompanyContactsValues values, DateTimeOffset now, string editor)
    {
        Apply(values, now);
        UpdatedBy = EditorName.Normalize(editor);
    }

    private void Apply(CompanyContactsValues values, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(values);
        SalesPhone = Phone(values.SalesPhone, nameof(SalesPhone));
        Email = Checked(values.Email, nameof(Email), EmailMaxLength, IsValidEmail);
        TelegramUrl = Checked(values.TelegramUrl, nameof(TelegramUrl), UrlMaxLength, IsValidTelegramUrl);
        ViberPhone = Phone(values.ViberPhone, nameof(ViberPhone));
        MapUrl = Checked(values.MapUrl, nameof(MapUrl), UrlMaxLength, IsValidHttpsUrl);
        CityUk = Checked(values.CityUk, nameof(CityUk), CityMaxLength, _ => true);
        CityRu = Checked(values.CityRu, nameof(CityRu), CityMaxLength, _ => true);
        UpdatedAt = now;
    }

    public static bool IsValidPhone(string? value) => value is not null && PhonePattern().IsMatch(value);

    public static bool IsValidEmail(string? value) => value is not null && EmailPattern().IsMatch(value);

    public static bool IsValidHttpsUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
           && !string.IsNullOrEmpty(uri.Host);

    public static bool IsValidTelegramUrl(string? value)
        => IsValidHttpsUrl(value) && new Uri(value!).Host is "t.me" or "telegram.me";

    private static string Phone(string? value, string field)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return IsValidPhone(trimmed)
            ? trimmed
            : throw new DomainException(CompanyContactsErrors.Invalid(field, value));
    }

    private static string Checked(string? value, string field, int maxLength, Func<string, bool> isValid)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new DomainException(CompanyContactsErrors.Required(field));
        if (trimmed.Length > maxLength)
            throw new DomainException(CompanyContactsErrors.TooLong(field, maxLength, trimmed.Length));
        return isValid(trimmed) ? trimmed : throw new DomainException(CompanyContactsErrors.Invalid(field, value));
    }

    [GeneratedRegex(@"^\+380\d{9}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();
}
