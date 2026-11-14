namespace Content.Domain.Contacts;

public static class CompanyContactsErrors
{
    public static DomainError Required(string field) =>
        new($"Content.Contacts.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Contacts.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError Invalid(string field) =>
        new($"Content.Contacts.{field}.Invalid", "Некоректне значення.", Field: field);
}
