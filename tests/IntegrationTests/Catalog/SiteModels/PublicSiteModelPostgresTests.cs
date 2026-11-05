using Catalog.Application.Features.SiteModels.Public.Common;
using Catalog.Application.Features.SiteModels.Public.GetList;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Application.Slugs;
using Catalog.Contracts.SiteModels.Public;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.Entities.Groups;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using IntegrationTests.Catalog.ProductMedia;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Catalog.SiteModels;

/// <summary>The public model catalog reads and filters on real PostgreSQL (spec catalog/007 stage 5).</summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class PublicSiteModelPostgresTests(CatalogPostgresFixture postgres)
{
    private const string Root = "000005513";
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);

    [SkippableFact]
    public async Task Reader_BuildsModelsAndFilters_FromTheDatabase()
    {
        postgres.SkipIfUnavailable();
        await using (var db = postgres.CreateContext())
        {
            await db.Database.ExecuteSqlRawAsync(
                """TRUNCATE "CatalogProductGroups", "CatalogAttributes", "ProductMedia", "ProductListProjections", "ProductPrices", "ProductTranslations", "Products", "CatalogSlugHistory", "CategoryFaqItems", "CategoryContents", "ProductCategories" CASCADE;""");
            db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(10), Root, "Завіси", "zavisy", CategoryPath.From("n10")));
            foreach (var id in new[] { 801, 802, 803 })
            {
                var product = Product.Create(ProductId.From(id), Root, $"Завіса {id}", $"zavisa-{id}",
                    ProductCategoryId.From(10), Now, 5, 1, false, false, true);
                db.Products.Add(product);
                db.ProductListProjections.Add(ProductListProjection.Create(product.Id, product.Name, product.Name,
                    product.ProductSlug, string.Empty, ProductCategoryId.From(10), "zavisy",
                    inStock: true, isSale: false, isNew: false, exportToSite: true, retailPrice: id));
            }

            var side = CatalogAttribute.Create("side", "Сторона", AttributeKind.Enum, null, "storona", true, true, 1, Now);
            db.Attributes.Add(side);
            await db.SaveChangesAsync();
            var left = AttributeValue.Create(side.Id, "L", "Ліва", "liva", 1, Now);
            var right = AttributeValue.Create(side.Id, "R", "Права", "prava", 2, Now);
            db.AttributeValues.AddRange(left, right);
            var model = ProductGroup.CreateManual(ProductCategoryId.From(10), "Завіса KEDR", "zavisa-kedr", Now);
            db.ProductGroups.Add(model);
            db.CategoryAttributes.Add(CategoryAttribute.Create(ProductCategoryId.From(10), side.Id, true, 1, Now));
            await db.SaveChangesAsync();

            db.ProductGroupMembers.AddRange(ProductGroupMember.CreatePinned(ProductId.From(801), model.Id, Now),
                ProductGroupMember.CreatePinned(ProductId.From(802), model.Id, Now));
            db.ProductGroupAxes.Add(ProductGroupAxis.Create(model.Id, side.Id, 0));
            db.ProductAttributeValues.AddRange(
                ProductAttributeValue.CreateManual(ProductId.From(801), side.Id, 0, left.Id, null, Now),
                ProductAttributeValue.CreateManual(ProductId.From(802), side.Id, 0, right.Id, null, Now),
                ProductAttributeValue.CreateManual(ProductId.From(803), side.Id, 0, left.Id, null, Now));
            await db.SaveChangesAsync();
        }

        await using var context = postgres.CreateContext();
        var reader = new PublicModelCatalogReader(context, new FakeProductMediaLibrary(), Options.Create(new RootCategoryId
        {
            DoorsRootCategoryId = "000007226", HardwareRootCategoryId = Root, CosmosRootCategoryId = "Космос",
            CosmosCategoryId = 920000
        }));
        var result = await new GetPublicSiteModelsQueryHandler(reader, new SlugService(context))
            .Handle(new GetPublicSiteModelsQuery("uk", "zavisy", new GetPublicSiteModelsRequest
            {
                Filter = ["storona:prava"]
            }), CancellationToken.None);

        var card = Assert.Single(result.Value.Items);
        Assert.Equal(("zavisa-kedr", 802m), (card.Slug, card.PriceFrom));
        var filter = Assert.Single(result.Value.Filters);
        Assert.Equal([("liva", 2), ("prava", 1)], filter.Values.Select(value => (value.Slug, value.Count)));
    }
}
