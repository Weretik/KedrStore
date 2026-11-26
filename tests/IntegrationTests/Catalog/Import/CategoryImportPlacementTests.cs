using Catalog.Domain.Categories.Tree;
using BuildingBlocks.Domain.Identifiers;
using Catalog.Application.Features.OneCSync.Source;
using Catalog.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Import;

/// <summary>Spec catalog/012 SC-MC05: the 1C sync keeps a place set by hand and never deletes a manual category.</summary>
public sealed class CategoryImportPlacementTests
{
    [Fact]
    public async Task Sync_KeepsAMovedCategory_PutsItsNewSubcategoryUnderIt_AndKeepsAManualCategory()
    {
        await using var db = CreateContext();
        var oneC = new FakeOneC
        {
            Categories =
            [
                new OneCCategoryDto(901, 5513, "Замки", ""),
                new OneCCategoryDto(902, 901, "Врізні", ""),
                new OneCCategoryDto(903, 5513, "Ручки", "")
            ]
        };
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);
        var now = DateTimeOffset.UtcNow;
        var tree = await db.Categories.IgnoreQueryFilters().ToListAsync();
        var handles = tree.Single(category => category.Id.Value == 903);
        var locks = tree.Single(category => category.Id.Value == 901);
        var mortise = tree.Single(category => category.Id.Value == 902);
        var manual = ProductCategory.CreateManual(ProductCategoryId.Create(800_000_000), "Накладні", "Накладные",
            "nakladni", mortise, null, now).Value;
        db.Categories.Add(manual);
        locks.MoveUnder(handles, now);
        CategoryTreePlacement.Rebuild([.. tree, manual], now);
        await db.SaveChangesAsync();

        oneC.Categories =
        [
            new OneCCategoryDto(901, 5513, "Замки дверні", ""),
            new OneCCategoryDto(902, 901, "Врізні", ""),
            new OneCCategoryDto(903, 5513, "Ручки", ""),
            new OneCCategoryDto(904, 901, "Навісні", "")
        ];
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);

        var after = await db.Categories.IgnoreQueryFilters().AsNoTracking()
            .ToDictionaryAsync(category => category.Id.Value);
        var handlesPath = after[903].Path.Value;
        Assert.Equal(903, after[901].ParentId!.Value.Value);
        Assert.True(after[901].IsPlacedByHand);
        Assert.Equal("Замки дверні", after[901].Name);
        Assert.Equal($"{handlesPath}.n901", after[901].Path.Value);
        Assert.Equal($"{handlesPath}.n901.n902", after[902].Path.Value);
        Assert.Equal($"{handlesPath}.n901.n904", after[904].Path.Value);
        Assert.Equal($"{handlesPath}.n901.n902.n800000000", after[800_000_000].Path.Value);
        Assert.False(after[800_000_000].IsDeleted);
    }
}
