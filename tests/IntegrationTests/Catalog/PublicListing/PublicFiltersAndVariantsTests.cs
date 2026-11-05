using Ardalis.Result;
using Catalog.Application.Features.Products.GetFacets;
using Catalog.Application.Features.Products.GetPublicList;
using Catalog.Contracts.Products.Facets;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;
using static IntegrationTests.Catalog.PublicListing.PublicListingTestSupport;

namespace IntegrationTests.Catalog.PublicListing;

/// <summary>
/// Spec catalog/006 phase 5: attribute filters, one card per variant group, facet counts, and product variants.
/// Products 1 (AB, left) and 2 (CR, left; CR picked by the assistant, not confirmed) form the group «Ручка»;
/// product 3 (AB, right) stands alone.
/// </summary>
public sealed class PublicFiltersAndVariantsTests
{
    private const string Category = "katehoriia-10";

    private static async Task<CatalogDbContext> CatalogAsync()
    {
        var db = CreateContext();
        AddCategory(db, 10);
        AddProduct(db, 1, 10, "Ручка AB L");
        AddProduct(db, 2, 10, "Ручка CP L");
        AddProduct(db, 3, 10, "Завіса AB R");
        foreach (var id in new[] { 1, 2, 3 })
            db.ProductListProjections.Add(ProductListProjection.Create(ProductId.From(id), $"Товар {id}", $"Товар {id}",
                $"product-{id}", "", ProductCategoryId.From(10), Category, inStock: true, isSale: false, isNew: false,
                exportToSite: true, retailPrice: 100 + id));
        await db.SaveChangesAsync();

        var csv = CsvHeader + "\n" + string.Join("\n", Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "CR", "L"),
            Row(3, "Завіса", "AB", "R")) + "\n";
        Assert.Empty((await RunAsync(db, Dictionary, csv, apply: true)).Errors);
        return db;
    }

    private static async Task<Result<PagedResult<List<ProductListRowDto>>>> ListAsync(CatalogDbContext db,
        bool groupVariants, params string[] filter)
        => await ListHandler(db).Handle(new GetPublicProductListQuery(new GetProductsRequest
        {
            CategorySlug = Category, Filter = filter, GroupVariants = groupVariants
        }), CancellationToken.None);

    private static async Task<Result<CategoryFacetsDto>> FacetsAsync(CatalogDbContext db, params string[] filter)
        => await FacetsHandler(db).Handle(new GetCategoryFacetsQuery(new GetProductsRequest
        {
            CategorySlug = Category, Filter = filter
        }), CancellationToken.None);

    private static Dictionary<string, int> Counts(CategoryFacetsDto facets, string slug)
        => facets.Facets.Single(facet => facet.Slug == slug).Values.ToDictionary(value => value.Slug, value => value.Count);

    [Fact]
    public async Task Filter_MatchesOnlyConfirmedValues_AnyValueOfOneAttribute_AllAttributes()
    {
        await using var db = await CatalogAsync();

        Assert.Equal([1, 3], (await ListAsync(db, false, "kolir:antychna-bronza")).Value.Value.Select(row => row.Id));
        Assert.Empty((await ListAsync(db, false, "kolir:khrom")).Value.Value);
        Assert.Equal([1, 3], (await ListAsync(db, false, "kolir:antychna-bronza,khrom")).Value.Value.Select(row => row.Id));
        Assert.Equal([3], (await ListAsync(db, false, "kolir:antychna-bronza", "storona:prava")).Value.Value
            .Select(row => row.Id));

        Assert.Equal(ResultStatus.NotFound, (await ListAsync(db, false, "kolir:nemaie")).Status);
        Assert.Equal(ResultStatus.NotFound, (await ListAsync(db, false, "nemaie:khrom")).Status);
        Assert.Equal(ResultStatus.Invalid, (await ListAsync(db, false, "kolir")).Status);
    }

    [Fact]
    public async Task GroupVariants_OneCardPerGroup_WithCountAndColors()
    {
        await using var db = await CatalogAsync();

        var plain = await ListAsync(db, false);
        var grouped = await ListAsync(db, true);

        Assert.Equal(3, plain.Value.PagedInfo.TotalRecords);
        Assert.Null(plain.Value.Value[0].VariantCount);
        Assert.Equal(2, grouped.Value.PagedInfo.TotalRecords);
        var group = Assert.Single(grouped.Value.Value, row => row.VariantCount is not null);
        Assert.Equal(2, group.VariantCount);
        var swatch = Assert.Single(group.Swatches!);
        Assert.Equal(("antychna-bronza", "#8C6A3F"), (swatch.Slug, swatch.Hex));
        Assert.Null(Assert.Single(grouped.Value.Value, row => row.Id == 3).Swatches);

        // With a filter the card shows a matching variant of the group.
        var left = await ListAsync(db, true, "storona:liva");
        Assert.Equal(1, Assert.Single(left.Value.Value).Id);
    }

    [Fact]
    public async Task Facets_CountCards_ApplyOtherAttributesButNotTheirOwn()
    {
        await using var db = await CatalogAsync();

        var all = (await FacetsAsync(db)).Value;
        Assert.Equal(2, all.TotalCards);
        Assert.Equal(["kolir", "storona"], all.Facets.Select(facet => facet.Slug).Order());
        Assert.Equal(new Dictionary<string, int> { ["antychna-bronza"] = 2 }, Counts(all, "kolir"));
        Assert.Equal(new Dictionary<string, int> { ["liva"] = 1, ["prava"] = 1 }, Counts(all, "storona"));
        Assert.Equal("swatch", all.Facets.Single(facet => facet.Slug == "kolir").Display);

        var right = (await FacetsAsync(db, "storona:prava")).Value;
        Assert.Equal(1, right.TotalCards);
        Assert.Equal(new Dictionary<string, int> { ["antychna-bronza"] = 1 }, Counts(right, "kolir"));
        Assert.Equal(new Dictionary<string, int> { ["liva"] = 1, ["prava"] = 1 }, Counts(right, "storona"));
        Assert.True(right.Facets.Single(facet => facet.Slug == "storona").Values.Single(value => value.Slug == "prava").Selected);

        Assert.Equal(ResultStatus.NotFound, (await FacetsHandler(db).Handle(new GetCategoryFacetsQuery(
            new GetProductsRequest { CategorySlug = "nemaie" }), CancellationToken.None)).Status);
    }

    [Fact]
    public async Task ProductVariants_ShowDifferingAxes_AndConfirmedCharacteristics()
    {
        await using var db = await CatalogAsync();

        var (unconfirmed, characteristics) = await VariantsReader(db).ReadAsync(1, "uk", CancellationToken.None);
        Assert.NotNull(unconfirmed);
        Assert.Empty(unconfirmed.Axes);
        Assert.Equal(["color", "side"], characteristics.Select(item => item.Code).Order());
        Assert.Equal("Антична бронза", characteristics.Single(item => item.Code == "color").Value);

        var waiting = await db.ProductAttributeValues.Where(value => value.ProductId == ProductId.From(2) && !value.IsConfirmed)
            .ToListAsync();
        Assert.All(waiting, value => value.Confirm(DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (variants, _) = await VariantsReader(db).ReadAsync(1, "uk", CancellationToken.None);
        var axis = Assert.Single(variants!.Axes);
        Assert.Equal(("color", "swatch"), (axis.Code, axis.Display));
        Assert.Equal(["antychna-bronza", "khrom"], axis.Values.Select(value => value.Key));
        Assert.Equal(2, variants.Items.Count);
        Assert.True(variants.Items.Single(item => item.Id == 1).IsCurrent);
        Assert.Equal("khrom", variants.Items.Single(item => item.Id == 2).Values["color"]);

        var (single, _) = await VariantsReader(db).ReadAsync(3, "uk", CancellationToken.None);
        Assert.Null(single);
    }
}
