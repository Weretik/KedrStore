using BuildingBlocks.Domain.Abstractions.Errors;

namespace Content.Domain.Seo;

public static class PageSeoErrors
{
    public static DomainError UnknownPage(string page) =>
        new("Content.PageSeo.Page.Unknown", $"Невідома сторінка сайту: {page}.", ErrorType.NotFound);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.PageSeo.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);
}
