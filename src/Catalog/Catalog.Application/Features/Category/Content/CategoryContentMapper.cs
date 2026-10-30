using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.Content;

public static class CategoryContentMapper
{
    public static CategoryContentResponse? ToPublic(CategoryContent? content)
        => content is null || content.IsEmpty
            ? null
            : new CategoryContentResponse(content.SeoTitle, content.MetaDescription, content.H1, content.BodyHtml,
                Faq(content), content.UpdatedAt);

    public static AdminCategoryContentItem ToAdmin(CategoryContent content)
        => new(content.Language, content.SeoTitle, content.MetaDescription, content.H1, content.BodyHtml,
            Faq(content), content.UpdatedAt, content.UpdatedBy);

    private static IReadOnlyList<CategoryFaqItemDto> Faq(CategoryContent content)
        => content.Faq.OrderBy(item => item.SortOrder).Select(item => new CategoryFaqItemDto(item.Question, item.Answer)).ToList();
}
