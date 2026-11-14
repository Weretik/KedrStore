using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.OneCSync.Sync;

/// <summary>Translations of the products in one language, including soft-deleted ones, so they can be revived.</summary>
public sealed class ProductTranslationsByProductIdsAndLanguageSpec : Specification<ProductTranslation>
{
    public ProductTranslationsByProductIdsAndLanguageSpec(IEnumerable<ProductId> productIds, string language)
    {
        Query
            .IgnoreQueryFilters()
            .Where(x => productIds.Contains(x.ProductId) && x.Language == language);
    }
}
