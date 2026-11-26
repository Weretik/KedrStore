using Catalog.Application.Abstractions.Media;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Category;
using Catalog.Application.Features.Categories.Shared.Tree;

namespace Catalog.Application.Features.Categories.Queries.GetAdminCategoryById;

/// <summary>One category for the admin, with the CDN address of its link-preview picture (spec catalog/010 R-03).</summary>
public sealed class GetAdminCategoryByIdHandler(
    ICatalogReadRepository<ProductCategory> repository,
    IProductMediaLibrary media)
    : IQueryHandler<GetAdminCategoryByIdQuery, Result<AdminCategoryResponse>>
{
    public async ValueTask<Result<AdminCategoryResponse>> Handle(GetAdminCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(new AllCategoriesSpec(null), cancellationToken);
        var category = categories.FirstOrDefault(x => x.Id.Value == query.Id);
        if (category is null)
            return Result.NotFound();

        var url = await CategoryShareImage.UrlAsync(media, category.ShareImageMediaFileId, cancellationToken);
        return Result.Success(CategoryReadMapper.ToAdminResponse(category, url));
    }
}
