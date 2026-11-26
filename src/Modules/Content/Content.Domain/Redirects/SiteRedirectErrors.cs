using BuildingBlocks.Domain.Abstractions.Errors;

namespace Content.Domain.Redirects;

public static class SiteRedirectErrors
{
    public static DomainError Required(string field) =>
        new($"Content.SiteRedirect.{field}.Required", "Вкажіть адресу.", Field: field);

    public static DomainError TooLong(string field, int maxLength) =>
        new($"Content.SiteRedirect.{field}.TooLong", $"Не більше {maxLength} символів.", Field: field);

    public static DomainError NotAPath(string field) =>
        new($"Content.SiteRedirect.{field}.NotAPath",
            "Адреса має починатися з «/» або з https://, без пробілів.", Field: field);

    public static DomainError ReservedPath(string field) =>
        new($"Content.SiteRedirect.{field}.Reserved",
            "Цю адресу не можна переадресувати: головна сторінка, /api/ і /_next/ належать сайту.", Field: field);

    public static DomainError SameAddress() =>
        new("Content.SiteRedirect.To.Same", "Нова адреса збігається зі старою.", Field: "to");

    public static DomainError Duplicate() =>
        new("Content.SiteRedirect.From.Duplicate", "Для цієї адреси переадресація вже є.", ErrorType.Conflict,
            Field: "from");

    public static DomainError Chain() =>
        new("Content.SiteRedirect.Chain",
            "Ланцюжок переадресацій: нова адреса сама переадресовується, або на стару адресу вже веде інша переадресація. Вкажіть кінцеву адресу.",
            Field: "to");

    public static DomainError LimitReached(int limit) =>
        new("Content.SiteRedirect.Limit", $"Не більше {limit} переадресацій.", ErrorType.Conflict);
}
