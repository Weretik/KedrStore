using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Category;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Categories.Queries.GetCategoryContent;

public sealed class GetCategoryContentHandler(IReadCatalogDbContext catalogDbContext)
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
