using BuildingBlocks.Domain.Identifiers;
using Catalog.Application.Features.Categories.Queries.GetAdminCategories;
using Catalog.Application.Features.Products.Shared;
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
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Catalog.Categories;

/// <summary>Product counts and filled text languages of the admin category list on real PostgreSQL.</summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class AdminCategoryInsightsPostgresTests(CatalogPostgresFixture postgres)
{
    private const string Root = "000005513";
    private static readonly DateTimeOffset Created = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    [SkippableFact]
    public async Task Reader_CountsListedProductsAndModels_AndNamesLanguagesWithAnyText()
    {
        postgres.SkipIfUnavailable();
        await using (var db = postgres.CreateContext())
        {
            await db.Database.ExecuteSqlRawAsync(
                """TRUNCATE catalog."ProductMedia", catalog."ProductListProjections", catalog."ProductPrices", catalog."ProductTranslations", catalog."Products", catalog."CatalogSlugHistory", catalog."CategoryFaqItems", catalog."CategoryContents", catalog."CatalogProductGroups", catalog."ProductCategories" CASCADE;""");
            db.Categories.Add(ProductCategory.Create(ProductCategoryId.Create(10), Root, "Замки", "zamky", CategoryPath.Create("n10")).Value);
            db.Categories.Add(ProductCategory.Create(ProductCategoryId.Create(11), Root, "Завіси", "zavisy", CategoryPath.Create("n11")).Value);
            foreach (var (id, exported) in new[] { (801, true), (802, false) })
            {
                var product = Product.Create(ProductId.Create(id), Root, $"Замок {id}", $"zamok-{id}",
                    ProductCategoryId.Create(10), Created, 5, 1, false, false, exported).Value;
                db.Products.Add(product);
                db.ProductListProjections.Add(ProductListProjection.Create(product.Id, product.Name, product.Name,
                    product.ProductSlug, string.Empty, ProductCategoryId.Create(10), "zamky",
                    inStock: true, isSale: false, isNew: false, exportToSite: exported, retailPrice: 100));
            }

            var withFaqOnly = CategoryContent.Create(ProductCategoryId.Create(10), "ru");
            withFaqOnly.Replace(null, null, null, null, [("Питання?", "Відповідь.")], "admin", Created);
            var withTitle = CategoryContent.Create(ProductCategoryId.Create(10), "uk");
            withTitle.Replace("Дверні замки", null, null, null, [], "admin", Created);
            var cleared = CategoryContent.Create(ProductCategoryId.Create(11), "uk");
            cleared.Replace(null, null, null, null, [], "admin", Created);
            db.CategoryContents.AddRange(withFaqOnly, withTitle, cleared);
            db.ProductGroups.Add(ProductGroup.CreateManual(ProductCategoryId.Create(10), "Замок KEDR", "zamok-kedr", Created).Value);
            db.ProductGroups.Add(ProductGroup.CreateAutomatic(ProductCategoryId.Create(10), "Замок навісний", "zamok navisnyi",
                "zamok-navisnyi", Created).Value);
            await db.SaveChangesAsync();
        }

        await using var context = postgres.CreateContext();
        var reader = new AdminCategoryInsightsReader(context);

        var counts = await reader.LoadDirectProductCountsAsync(CancellationToken.None);
        var languages = await reader.LoadFilledContentLanguagesAsync(CancellationToken.None);
        var models = await reader.LoadDirectModelCountsAsync(CancellationToken.None);

        Assert.Equal(2, counts[10]);
        Assert.False(counts.ContainsKey(11));
        Assert.Equal(["ru", "uk"], languages[10]);
        Assert.False(languages.ContainsKey(11));
        Assert.Equal(new CategoryModelCounts(2, 1), models[10]);
        Assert.False(models.ContainsKey(11));
    }
}
