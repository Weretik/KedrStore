using Catalog.Application.Contracts.Content;
using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Category;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Category.Content.Save;

/// <summary>Admin replaces one language of a category's content; the body is sanitized before it is stored.</summary>
public sealed class SaveCategoryContentCommandHandler(
    ICatalogRepository<CategoryContent> contentRepo,
    IReadCatalogDbContext catalogDbContext,
    IHtmlSanitizerService sanitizer)
    : ICommandHandler<SaveCategoryContentCommand, Result<AdminCategoryContentItem>>
{
    public async ValueTask<Result<AdminCategoryContentItem>> Handle(SaveCategoryContentCommand command,
        CancellationToken cancellationToken)
    {
        var categoryId = ProductCategoryId.From(command.CategoryId);
        if (!await catalogDbContext.Categories.AsNoTracking().AnyAsync(category => category.Id == categoryId, cancellationToken))
            return Result.NotFound();

        var language = command.Language.Trim().ToLowerInvariant();
        var content = await contentRepo.FirstOrDefaultAsync(new CategoryContentSpec(categoryId, language), cancellationToken);
        var isNew = content is null;
        content ??= CategoryContent.Create(categoryId, language);

        var request = command.Request;
        var body = string.IsNullOrWhiteSpace(request.BodyHtml) ? null : sanitizer.Sanitize(request.BodyHtml);
        var faq = (request.Faq ?? []).Select(item => (item.Question, item.Answer)).ToList();

        content.Replace(request.SeoTitle, request.MetaDescription, request.H1, body, faq, command.UserId, DateTimeOffset.UtcNow);

        if (isNew)
            await contentRepo.AddAsync(content, cancellationToken);
        else
            await contentRepo.SaveChangesAsync(cancellationToken);

        return Result.Success(CategoryContentMapper.ToAdmin(content));
    }
}

internal sealed class CategoryContentSpec : SingleResultSpecification<CategoryContent>
{
    public CategoryContentSpec(ProductCategoryId categoryId, string language)
        => Query.Where(content => content.CategoryId == categoryId && content.Language == language);
}
