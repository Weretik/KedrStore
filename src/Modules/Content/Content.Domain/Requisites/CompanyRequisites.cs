using System.Text.RegularExpressions;

namespace Content.Domain.Requisites;

/// <summary>The company's official details as the requisites page shows them.</summary>
public sealed record CompanyRequisitesValues(
    string LegalName,
    string Code,
    string RegistrationExtractNumber,
    string Iban,
    string BankName,
    string BankCode);

/// <summary>
/// The company's requisites (ФОП name, tax code, registration extract, IBAN, bank). One row
/// (<see cref="SingletonId"/>), created by a migration and only updated. A wrong IBAN costs money, so every
/// update reports which fields changed and the handler logs them in <see cref="CompanyRequisitesChange"/>.
/// </summary>
public sealed partial class CompanyRequisites : IAggregateRoot
{
    public const int SingletonId = 1;
    public const int LegalNameMaxLength = 200;
    public const int BankNameMaxLength = 120;
    public const int ExtractNumberMaxLength = 30;

    public int Id { get; private set; }
    public string LegalName { get; private set; } = null!;

    /// <summary>ЄДРПОУ (8 digits) or РНОКПП of a ФОП (10 digits).</summary>
    public string Code { get; private set; } = null!;
    public string RegistrationExtractNumber { get; private set; } = null!;

    /// <summary>Ukrainian IBAN without spaces, checksum verified.</summary>
    public string Iban { get; private set; } = null!;
    public string BankName { get; private set; } = null!;

    /// <summary>МФО of the bank, 6 digits; it must match the IBAN.</summary>
    public string BankCode { get; private set; } = null!;
    public DateTimeOffset UpdatedAt { get; private set; }

    private CompanyRequisites() { }

    public static DomainResult<CompanyRequisites> Create(CompanyRequisitesValues values, DateTimeOffset now)
    {
        var requisites = new CompanyRequisites { Id = SingletonId };
        var updated = requisites.Update(values, now);
        return updated.IsFailure ? updated.Error! : requisites;
    }

    /// <summary>Replaces all fields; returns the fields whose value changed (empty when nothing did).</summary>
    public DomainResult<IReadOnlyList<RequisitesFieldChange>> Update(CompanyRequisitesValues values, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(values);
        var normalized = Normalize(values);
        if (normalized.IsFailure)
            return normalized.Error!;
        var next = normalized.Value;
        var changes = new List<RequisitesFieldChange>();

        LegalName = Apply(nameof(LegalName), LegalName, next.LegalName, changes);
        Code = Apply(nameof(Code), Code, next.Code, changes);
        RegistrationExtractNumber = Apply(nameof(RegistrationExtractNumber), RegistrationExtractNumber,
            next.RegistrationExtractNumber, changes);
        Iban = Apply(nameof(Iban), Iban, next.Iban, changes);
        BankName = Apply(nameof(BankName), BankName, next.BankName, changes);
        BankCode = Apply(nameof(BankCode), BankCode, next.BankCode, changes);

        if (changes.Count > 0 || UpdatedAt == default)
            UpdatedAt = now;
        return changes;
    }

    /// <summary>IBAN as stored: spaces removed, upper case.</summary>
    public static string NormalizeIban(string? value)
        => (value ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    public static bool IsValidCode(string? value) => value is not null && CodePattern().IsMatch(value);

    public static bool IsValidExtractNumber(string? value)
        => value is not null && value.Length <= ExtractNumberMaxLength && DigitsPattern().IsMatch(value);

    public static bool IsValidBankCode(string? value) => value is not null && BankCodePattern().IsMatch(value);

    /// <summary>UA + 2 check digits + 25 digits (МФО, zeros, account), ISO 13616 mod-97 checksum.</summary>
    public static bool IsValidIban(string? value)
    {
        if (value is null || !IbanPattern().IsMatch(value))
            return false;
        var rearranged = value[4..] + value[..4];
        var remainder = 0;
        foreach (var symbol in rearranged)
        {
            var digits = char.IsLetter(symbol) ? (symbol - 'A' + 10).ToString() : symbol.ToString();
            foreach (var digit in digits)
                remainder = (remainder * 10 + (digit - '0')) % 97;
        }
        return remainder == 1;
    }

    /// <summary>The МФО is digits 5–10 of a Ukrainian IBAN.</summary>
    public static bool BankCodeMatchesIban(string? iban, string? bankCode)
        => iban is { Length: 29 } && bankCode is { Length: 6 } && iban.Substring(4, 6) == bankCode;

    private static DomainResult<CompanyRequisitesValues> Normalize(CompanyRequisitesValues values)
    {
        var checks = new DomainChecks();
        var legalName = checks.Take(Text(values.LegalName, nameof(LegalName), LegalNameMaxLength));
        var code = checks.Take(Checked(values.Code, nameof(Code), IsValidCode));
        var extract = checks.Take(Checked(values.RegistrationExtractNumber, nameof(RegistrationExtractNumber),
            IsValidExtractNumber));
        var iban = NormalizeIban(values.Iban);
        if (!IsValidIban(iban))
            checks.Check(iban.Length == 0
                ? CompanyRequisitesErrors.Required(nameof(Iban))
                : CompanyRequisitesErrors.Invalid(nameof(Iban)));
        var bankName = checks.Take(Text(values.BankName, nameof(BankName), BankNameMaxLength));
        var bankCode = checks.Take(Checked(values.BankCode, nameof(BankCode), IsValidBankCode));
        if (checks.Error is { } error)
            return error;
        if (!BankCodeMatchesIban(iban, bankCode))
            return CompanyRequisitesErrors.BankCodeMismatch();
        return new CompanyRequisitesValues(legalName, code, extract, iban, bankName, bankCode);
    }

    private static string Apply(string field, string? current, string next, List<RequisitesFieldChange> changes)
    {
        if (current is not null && current != next)
            changes.Add(new RequisitesFieldChange(field, current, next));
        return next;
    }

    private static DomainResult<string> Text(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return CompanyRequisitesErrors.Required(field);
        return trimmed.Length <= maxLength
            ? trimmed
            : CompanyRequisitesErrors.TooLong(field, maxLength, trimmed.Length);
    }

    private static DomainResult<string> Checked(string? value, string field, Func<string, bool> isValid)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return CompanyRequisitesErrors.Required(field);
        return isValid(trimmed) ? trimmed : CompanyRequisitesErrors.Invalid(field);
    }

    [GeneratedRegex(@"^(\d{8}|\d{10})$", RegexOptions.CultureInvariant)]
    private static partial Regex CodePattern();

    [GeneratedRegex(@"^\d+$", RegexOptions.CultureInvariant)]
    private static partial Regex DigitsPattern();

    [GeneratedRegex(@"^\d{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex BankCodePattern();

    [GeneratedRegex(@"^UA\d{27}$", RegexOptions.CultureInvariant)]
    private static partial Regex IbanPattern();
}
