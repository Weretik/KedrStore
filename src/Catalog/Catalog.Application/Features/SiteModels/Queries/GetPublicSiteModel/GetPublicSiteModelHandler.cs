using Catalog.Application.Features.SiteModels.Shared.Public;
using Catalog.Application.Shared;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Contracts.SiteModels.Public;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModel;

/// <summary>
/// The model page (spec catalog/007 stage 5). A current model slug, or the slug of an on-site product shown as a model
/// of its own (M-04), gives the page. A former model slug, or the slug of a product inside an accepted model, gives
/// the address to redirect to; anything else is not found.
/// </summary>
public sealed class GetPublicSiteModelHandler(PublicModelCatalogReader reader, ISlugService slugService)
    : IQueryHandler<GetPublicSiteModelQuery, Result<PublicSiteModelLookup>>
{
    public async ValueTask<Result<PublicSiteModelLookup>> Handle(GetPublicSiteModelQuery query,
        CancellationToken cancellationToken)
    {
        var slug = query.Slug.Trim().ToLowerInvariant();
        var language = CatalogLanguage.Normalize(query.Lang);
        var catalog = await reader.ReadAsync(cancellationToken);

        if (catalog.ModelBySlug.TryGetValue(slug, out var model))
            return Result.Success(new PublicSiteModelLookup(PublicModelPageMapper.ToPage(catalog, model, language), null));

        if (await slugService.ResolveAsync(CatalogEntityType.ProductGroup, slug, cancellationToken) is { } modelId
            && catalog.Models.FirstOrDefault(item => item.ModelId?.Value == modelId) is { } moved
            && catalog.Categories.TryGetValue(moved.CategoryId, out var category))
            return Result.Success(new PublicSiteModelLookup(null,
                new PublicSiteModelAddressDto(moved.Slug, category.Slug,
                    new Dictionary<string, string>())));

        if (await slugService.ResolveAsync(CatalogEntityType.Product, slug, cancellationToken) is { } productId
            && catalog.AddressOf(ProductId.From(productId)) is { } address)
            return Result.Success(new PublicSiteModelLookup(null, address));

        return Result.NotFound();
    }
}
