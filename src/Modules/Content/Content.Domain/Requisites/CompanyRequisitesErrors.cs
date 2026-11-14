using BuildingBlocks.Domain.Abstractions.Errors;

namespace Content.Domain.Requisites;

public static class CompanyRequisitesErrors
{
    public static DomainError Required(string field) =>
        new($"Content.Requisites.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Requisites.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError Invalid(string field) =>
        new($"Content.Requisites.{field}.Invalid", "Некоректне значення.", Field: field);

    public static DomainError BankCodeMismatch() =>
        new("Content.Requisites.BankCode.Mismatch", "МФО має збігатися з цифрами 5–10 IBAN.", Field: "BankCode");
}
