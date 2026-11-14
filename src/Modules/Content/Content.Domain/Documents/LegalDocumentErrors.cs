using BuildingBlocks.Domain.Abstractions.Errors;

namespace Content.Domain.Documents;

public static class LegalDocumentErrors
{
    public static DomainError LanguageMissing(string language) =>
        new("Content.Document.Text.Missing",
            language == "ru" ? "Додайте текст російською." : "Додайте текст українською.", Field: "Texts");

    public static DomainError LanguageInvalid() =>
        new("Content.Document.Text.Language", "Мова документа: лише українська або російська.", Field: "Texts");

    public static DomainError Required(string field) =>
        new($"Content.Document.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Document.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError NotDraft() =>
        DomainError.Conflict("Content.Document.NotDraft", "Цю редакцію вже опубліковано, змінити можна лише чернетку.");

    public static DomainError NotPublished() =>
        DomainError.Conflict("Content.Document.NotPublished", "Ця редакція зараз не на сайті.");

    public static DomainError EffectiveFromTooEarly(DateOnly current) =>
        new("Content.Document.EffectiveFrom.TooEarly",
            $"Дата не може бути раніше за чинну редакцію ({current:dd.MM.yyyy}).", Field: "effectiveFrom");
}
