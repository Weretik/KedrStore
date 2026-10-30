using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Category;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Category.Content.Get;

public sealed class GetCategoryContentQueryHandler(IReadCatalogDbContext catalogDbContext)
    : IQueryHandler<GetCategoryContentQuery, Result<AdminCategoryContentResponse>>
{
    public async ValueTask<Result<AdminCategoryContentResponse>> Handle(GetCategoryContentQuery query,
        CancellationToken cancellationToken)
    {
        var categoryId = ProductCategoryId.From(query.CategoryId);
        if (!await catalogDbContext.Categories.AsNoTracking().AnyAsync(category => category.Id == categoryId, cancellationToken))
            return Result.NotFound();

        var contents = await catalogDbContext.CategoryContents.AsNoTracking()
            .Where(content => content.CategoryId == categoryId)
            .OrderBy(content => content.Language)
            .ToListAsync(cancellationToken);

        return Result.Success(new AdminCategoryContentResponse(
            query.CategoryId,
            contents.Select(CategoryContentMapper.ToAdmin).ToList()));
    }
}
