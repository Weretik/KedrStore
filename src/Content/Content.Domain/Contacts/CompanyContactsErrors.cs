namespace Content.Domain.Contacts;

public static class CompanyContactsErrors
{
    public static DomainError Required(string field) =>
        new($"Content.Contacts.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Contacts.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError Invalid(string field, string? value) =>
        new($"Content.Contacts.{field}.Invalid", $"{field} has an invalid value: {value}");
}
