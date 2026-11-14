using Catalog.Application.Features.Category.SiteVisibility;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Options;
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
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Categories;

/// <summary>Spec catalog/007 R-18: the «show on site» mark of categories, from the 1C import and from an admin.</summary>
public sealed class CategorySiteVisibilityTests
{
    private const string Doors = "000007226";

    private static SetCategorySiteVisibilityCommandHandler Handler(CatalogDbContext db)
        => new(new TestRepository<ProductCategory>(db), Options.Create(new RootCategoryId
        {
            DoorsRootCategoryId = Doors,
            HardwareRootCategoryId = Root,
            CosmosRootCategoryId = "Космос",
            CosmosCategoryId = 920000
        }));

    private static async Task<Dictionary<int, bool>> ShownAsync(CatalogDbContext db)
        => await db.Categories.IgnoreQueryFilters().AsNoTracking()
            .ToDictionaryAsync(category => category.Id.Value, category => category.IsShownOnSite);

    private static Task SetAsync(CatalogDbContext db, int id, bool isShown)
        => Handler(db).Handle(new SetCategorySiteVisibilityCommand(id, isShown), CancellationToken.None).AsTask();

    [Fact]
    public async Task NewCategory_TakesItsParentsMark_HardwareShownDoorsHidden_SC_M14()
    {
        await using var db = CreateContext();
        var oneC = new FakeOneC
        {
            Categories = [new OneCCategoryDto(901, 5513, "Замки", ""), new OneCCategoryDto(911, 901, "Врізні", "")]
        };
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);
        Assert.All(await ShownAsync(db), pair => Assert.True(pair.Value));

        await SetAsync(db, 901, false);
        db.ChangeTracker.Clear();
        oneC.Categories = [.. oneC.Categories, new OneCCategoryDto(912, 901, "Накладні", "")];
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);
        var shown = await ShownAsync(db);
        Assert.True(shown[5513]);
        Assert.Equal((false, false, false), (shown[901], shown[911], shown[912]));

        await CategoryJob(db, new FakeOneC { Categories = [new OneCCategoryDto(931, 7226, "Міжкімнатні", "")] })
            .RunAsync(Doors, CancellationToken.None);
        Assert.False((await ShownAsync(db))[931]);
    }

    [Fact]
    public async Task Admin_HidesWithSubcategories_ShowsWithParents_NeverShowsCosmos_SC_M14()
    {
        await using var db = CreateContext();
        var oneC = new FakeOneC
        {
            Categories =
            [
                new OneCCategoryDto(901, 5513, "Замки", ""), new OneCCategoryDto(911, 901, "Врізні", ""),
                new OneCCategoryDto(912, 901, "Накладні", "")
            ]
        };
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);
        await SetAsync(db, 5513, false);
        Assert.All(await ShownAsync(db), pair => Assert.False(pair.Value));

        var shown = await Handler(db).Handle(new SetCategorySiteVisibilityCommand(911, true), CancellationToken.None);
        Assert.Equal(3, shown.Value.ChangedCount);
        var marks = await ShownAsync(db);
        Assert.Equal((true, true, true, false), (marks[5513], marks[901], marks[911], marks[912]));

        await CategoryJob(db, new FakeOneC()).RunAsync("Космос", CancellationToken.None);
        var cosmos = await Handler(db).Handle(new SetCategorySiteVisibilityCommand(920000, true), CancellationToken.None);
        Assert.False(cosmos.IsSuccess);
        Assert.False((await ShownAsync(db))[920000]);
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound,
            (await Handler(db).Handle(new SetCategorySiteVisibilityCommand(999, true), CancellationToken.None)).Status);
    }
}
