using Catalog.Domain.Entities;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Import;

/// <summary>SC-002, SC-003, SC-005 on the category import.</summary>
public sealed class CategoryImportSoftDeleteTests
{
    [Fact]
    public async Task Import_SoftDeletesMissingCategory_KeepsSlugAndContent_AndRestoresIt()
    {
        await using var db = CreateContext();
        var oneC = new FakeOneC { Categories = [.. Enumerable.Range(1, 5).Select(Row)] };
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);
        var content = CategoryContent.Create(ProductCategoryId.From(905), "uk");
        content.Replace("Заголовок", null, null, null, [], "admin", DateTimeOffset.UtcNow);
        db.CategoryContents.Add(content);
        await db.SaveChangesAsync();

        oneC.Categories = [.. Enumerable.Range(1, 4).Select(Row)];
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);

        var deleted = await db.Categories.IgnoreQueryFilters().SingleAsync(category => category.Id == ProductCategoryId.From(905));
        Assert.True(deleted.IsDeleted);
        Assert.Equal("katehoriia-5", deleted.Slug);
        Assert.False(await db.Categories.AnyAsync(category => category.Id == ProductCategoryId.From(905)));
        Assert.True(await db.CategoryContents.AnyAsync(item => item.CategoryId == ProductCategoryId.From(905)));

        oneC.Categories = [.. Enumerable.Range(1, 5).Select(Row)];
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);

        var restored = await db.Categories.SingleAsync(category => category.Id == ProductCategoryId.From(905));
        Assert.Equal("katehoriia-5", restored.Slug);
    }

    [Fact]
    public async Task Import_GivesSameNamedNewCategoryACollisionSuffix_AndKeepsSlugOnRename()
    {
        await using var db = CreateContext();
        var oneC = new FakeOneC { Categories = [new OneCCategoryDto(901, 5513, "Замки", ""), new OneCCategoryDto(902, 5513, "Замки", "")] };
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);

        oneC.Categories = [new OneCCategoryDto(901, 5513, "Замки врізні", ""), new OneCCategoryDto(902, 5513, "Замки", "")];
        await CategoryJob(db, oneC).RunAsync(Root, CancellationToken.None);

        var slugs = await db.Categories.ToDictionaryAsync(category => category.Id.Value, category => category.Slug);
        Assert.Equal("zamky", slugs[901]);
        Assert.Equal("zamky-2", slugs[902]);
        Assert.Equal("furnitra", slugs[5513]);
    }

    private static OneCCategoryDto Row(int index) => new(900 + index, 5513, $"Категорія {index}", "");
}
