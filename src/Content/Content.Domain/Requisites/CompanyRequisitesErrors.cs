namespace Content.Domain.Requisites;

public static class CompanyRequisitesErrors
{
    public static DomainError Required(string field) =>
        new($"Content.Requisites.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Requisites.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError Invalid(string field, string? value) =>
        new($"Content.Requisites.{field}.Invalid", $"{field} has an invalid value: {value}");

    public static DomainError BankCodeMismatch() =>
        new("Content.Requisites.BankCode.Mismatch", "BankCode must equal digits 5-10 of the IBAN");

    public static DomainError NothingChanged() =>
        new("Content.Requisites.NothingChanged", "A change log entry needs at least one changed field");
}
