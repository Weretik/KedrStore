using Catalog.Application.Features.Categories.Queries.GetAdminCategories;
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
    public async Task Reader_CountsListedProducts_AndNamesLanguagesWithAnyText()
    {
        postgres.SkipIfUnavailable();
        await using (var db = postgres.CreateContext())
        {
            await db.Database.ExecuteSqlRawAsync(
                """TRUNCATE "ProductMedia", "ProductListProjections", "ProductPrices", "ProductTranslations", "Products", "CatalogSlugHistory", "CategoryFaqItems", "CategoryContents", "ProductCategories" CASCADE;""");
            db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(10), Root, "Замки", "zamky", CategoryPath.From("n10")).Value);
            db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(11), Root, "Завіси", "zavisy", CategoryPath.From("n11")).Value);
            foreach (var (id, exported) in new[] { (801, true), (802, false) })
            {
                var product = Product.Create(ProductId.From(id), Root, $"Замок {id}", $"zamok-{id}",
                    ProductCategoryId.From(10), Created, 5, 1, false, false, exported).Value;
                db.Products.Add(product);
                db.ProductListProjections.Add(ProductListProjection.Create(product.Id, product.Name, product.Name,
                    product.ProductSlug, string.Empty, ProductCategoryId.From(10), "zamky",
                    inStock: true, isSale: false, isNew: false, exportToSite: exported, retailPrice: 100));
            }

            var withFaqOnly = CategoryContent.Create(ProductCategoryId.From(10), "ru");
            withFaqOnly.Replace(null, null, null, null, [("Питання?", "Відповідь.")], "admin", Created);
            var withTitle = CategoryContent.Create(ProductCategoryId.From(10), "uk");
            withTitle.Replace("Дверні замки", null, null, null, [], "admin", Created);
            var cleared = CategoryContent.Create(ProductCategoryId.From(11), "uk");
            cleared.Replace(null, null, null, null, [], "admin", Created);
            db.CategoryContents.AddRange(withFaqOnly, withTitle, cleared);
            await db.SaveChangesAsync();
        }

        await using var context = postgres.CreateContext();
        var reader = new AdminCategoryInsightsReader(context);

        var counts = await reader.LoadDirectProductCountsAsync(CancellationToken.None);
        var languages = await reader.LoadFilledContentLanguagesAsync(CancellationToken.None);

        Assert.Equal(2, counts[10]);
        Assert.False(counts.ContainsKey(11));
        Assert.Equal(["ru", "uk"], languages[10]);
        Assert.False(languages.ContainsKey(11));
    }
}
