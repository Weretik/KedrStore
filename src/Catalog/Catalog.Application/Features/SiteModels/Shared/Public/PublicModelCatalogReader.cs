using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Shared.Public;

/// <summary>
/// Reads <see cref="PublicModelCatalog"/> without tracking. Hidden categories (R-18), the Cosmos import root (owner
/// decision 2026-11-01), and everything under them stay out with their products; only values a person confirmed count
/// (spec catalog/006 R-06).
/// </summary>
public sealed class PublicModelCatalogReader(
    IReadCatalogDbContext db,
    IProductMediaLibrary media,
    IOptions<RootCategoryId> roots)
{
    public async Task<PublicModelCatalog> ReadAsync(CancellationToken cancellationToken)
    {
        var categories = await ReadCategoriesAsync(cancellationToken);
        var products = (await (from projection in db.ProductListProjections.AsNoTracking()
                    where projection.ExportToSite
                    join product in db.Products.AsNoTracking() on projection.ProductId equals product.Id
                    select new PublicProductFacts(projection.ProductId, projection.NameUk, projection.NameRu,
                        projection.ProductSlug, projection.CategoryId, projection.Photo, projection.RetailPrice,
                        projection.InStock, projection.IsSale, projection.IsNew))
                .ToListAsync(cancellationToken))
            .Where(product => categories.ContainsKey(product.CategoryId))
            .ToDictionary(product => product.Id);

        var accepted = await db.ProductGroups.AsNoTracking()
            .Where(model => model.Status == ProductGroupStatus.Accepted)
            .Select(model => new { model.Id, model.Name, model.Slug, model.CategoryId, model.MainProductId })
            .ToListAsync(cancellationToken);
        var acceptedIds = accepted.Select(model => model.Id).ToList();
        var members = (await db.ProductGroupMembers.AsNoTracking()
                .Where(member => member.GroupId != null && acceptedIds.Contains(member.GroupId.Value))
                .Select(member => new { member.ProductId, GroupId = member.GroupId!.Value })
                .ToListAsync(cancellationToken))
            .ToLookup(member => member.GroupId, member => member.ProductId);
        var axes = (await db.ProductGroupAxes.AsNoTracking()
                .Where(axis => acceptedIds.Contains(axis.GroupId))
                .ToListAsync(cancellationToken))
            .ToLookup(axis => axis.GroupId);

        var models = new List<PublicModelFacts>();
        foreach (var model in accepted)
        {
            var onSite = members[model.Id]
                .Where(id => products.TryGetValue(id, out var product) && product.CategoryId == model.CategoryId)
                .OrderBy(id => id.Value)
                .ToList();
            if (onSite.Count > 0)
                models.Add(new PublicModelFacts(model.Slug, model.Name, model.CategoryId, model.Id,
                    model.MainProductId, onSite,
                    axes[model.Id].OrderBy(axis => axis.Position).Select(axis => axis.AttributeId).ToList()));
        }

        var modelOfProduct = models.SelectMany(model => model.Products.Select(id => (id, model)))
            .ToDictionary(pair => pair.id, pair => pair.model);
        foreach (var product in products.Values.Where(product => !modelOfProduct.ContainsKey(product.Id)))
        {
            var own = new PublicModelFacts(product.Slug, product.NameUk, product.CategoryId, null, product.Id,
                [product.Id], []);
            models.Add(own);
            modelOfProduct[product.Id] = own;
        }

        // An accepted model keeps its slug when an own-product model has the same one.
        var modelBySlug = new Dictionary<string, PublicModelFacts>(StringComparer.Ordinal);
        foreach (var model in models.OrderByDescending(model => model.IsAccepted))
            modelBySlug.TryAdd(model.Slug, model);

        var productIds = products.Keys.ToList();
        var values = (await db.ProductAttributeValues.AsNoTracking()
                .Where(value => value.IsConfirmed && (value.ValueId != null || value.NumberValue != null)
                                                  && productIds.Contains(value.ProductId))
                .Select(value => new { value.ProductId, value.AttributeId, value.Position, value.ValueId, value.NumberValue })
                .ToListAsync(cancellationToken))
            .ToLookup(value => value.ProductId,
                value => new PublicProductValueFacts(value.AttributeId, value.Position, value.ValueId, value.NumberValue));

        var attributes = await db.Attributes.AsNoTracking()
            .Select(attribute => new PublicAttributeFacts(attribute.Id, attribute.Code, attribute.Slug,
                attribute.NameUk, attribute.NameRu, attribute.Kind, attribute.Unit, attribute.SortOrder))
            .ToDictionaryAsync(attribute => attribute.Id, cancellationToken);
        var dictionary = await ReadDictionaryAsync(cancellationToken);

        var filterableIds = await db.Attributes.AsNoTracking()
            .Where(attribute => attribute.IsFilterable && attribute.Kind != AttributeKind.Number)
            .Select(attribute => attribute.Id)
            .ToListAsync(cancellationToken);
        var categoryFilters = (await db.CategoryAttributes.AsNoTracking()
                .Where(filter => filter.IsFilterable && filterableIds.Contains(filter.AttributeId))
                .Select(filter => new PublicCategoryFilterFacts(filter.CategoryId, filter.AttributeId, filter.SortOrder))
                .ToListAsync(cancellationToken))
            .ToLookup(filter => filter.CategoryId);

        var texts = (await db.ProductGroupTexts.AsNoTracking()
                .Where(text => acceptedIds.Contains(text.GroupId))
                .Select(text => new PublicModelTextFacts(text.GroupId, text.Language, text.ShortDescription,
                    text.Description, text.SeoTitle, text.SeoDescription))
                .ToListAsync(cancellationToken))
            .ToLookup(text => text.ModelId);

        return new PublicModelCatalog
        {
            Products = products,
            Models = models,
            ModelBySlug = modelBySlug,
            ModelOfProduct = modelOfProduct,
            Values = values,
            Attributes = attributes,
            Dictionary = dictionary,
            Categories = categories,
            CategoryFilters = categoryFilters,
            Texts = texts
        };
    }

    /// <summary>Active categories shown on the site, outside the hidden import root, and not under a hidden one.</summary>
    private async Task<Dictionary<ProductCategoryId, PublicCategoryFacts>> ReadCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var rows = await db.Categories.AsNoTracking()
            .Select(category => new
            {
                Facts = new PublicCategoryFacts(category.Id, category.ParentId, category.Slug, category.ShortNameUk,
                    category.ShortNameRu),
                category.ProductTypeIdOneC,
                category.IsShownOnSite
            })
            .ToListAsync(cancellationToken);

        var hiddenRoot = roots.Value.CosmosRootCategoryId;
        var hidden = rows.Where(row => !row.IsShownOnSite || row.ProductTypeIdOneC == hiddenRoot)
            .Select(row => row.Facts.Id).ToHashSet();
        var childrenOf = rows.Where(row => row.Facts.ParentId is not null)
            .ToLookup(row => row.Facts.ParentId!.Value, row => row.Facts.Id);
        var pending = new Stack<ProductCategoryId>(hidden);
        while (pending.TryPop(out var id))
        {
            foreach (var child in childrenOf[id])
                if (hidden.Add(child))
                    pending.Push(child);
        }

        return rows.Where(row => !hidden.Contains(row.Facts.Id)).ToDictionary(row => row.Facts.Id, row => row.Facts);
    }

    private async Task<Dictionary<AttributeValueId, PublicValueFacts>> ReadDictionaryAsync(
        CancellationToken cancellationToken)
    {
        var rows = await db.AttributeValues.AsNoTracking()
            .Select(value => new
            {
                value.Id, value.AttributeId, value.Slug, value.NameUk, value.NameRu, value.SortOrder, value.SwatchHex,
                value.SwatchMediaFileId
            })
            .ToListAsync(cancellationToken);
        var mediaIds = rows.Select(row => row.SwatchMediaFileId).OfType<long>().Distinct().ToList();
        var textures = mediaIds.Count == 0
            ? new Dictionary<long, string>()
            : (await media.GetByIdsAsync(mediaIds, cancellationToken)).ToDictionary(file => file.Id, file => file.Url);

        return rows.ToDictionary(row => row.Id, row => new PublicValueFacts(row.Id, row.AttributeId, row.Slug,
            row.NameUk, row.NameRu, row.SortOrder, row.SwatchHex,
            row.SwatchMediaFileId is { } mediaId ? textures.GetValueOrDefault(mediaId) : null));
    }
}
