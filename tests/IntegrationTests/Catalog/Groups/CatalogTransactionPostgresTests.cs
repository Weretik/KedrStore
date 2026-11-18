using BuildingBlocks.Domain.Identifiers;
using Ardalis.Result;
using global::Catalog.Domain.Categories;
using global::Catalog.Domain.Groups;
using global::Catalog.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Catalog.Groups;

/// <summary>Database rules 3.1: a Catalog command that saves twice keeps both saves or neither.</summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class CatalogTransactionPostgresTests(CatalogPostgresFixture postgres)
{
    private const int CategoryId = 30;

    [SkippableFact]
    public async Task A_success_keeps_every_save()
    {
        postgres.SkipIfUnavailable();
        await SeedCategoryAsync();

        var result = await RunAsync(saved => Result.Success(saved));

        Assert.True(result.IsSuccess);
        Assert.Equal(["Перша", "Друга"], await GroupNamesAsync());
    }

    [SkippableFact]
    public async Task A_failure_after_a_save_rolls_it_back()
    {
        postgres.SkipIfUnavailable();
        await SeedCategoryAsync();

        var result = await RunAsync(_ => Result<int>.Invalid(new ValidationError("name", "Ні.")));

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Empty(await GroupNamesAsync());
    }

    [SkippableFact]
    public async Task An_exception_after_a_save_rolls_it_back()
    {
        postgres.SkipIfUnavailable();
        await SeedCategoryAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            RunAsync(_ => throw new InvalidOperationException("Зупинка між збереженнями.")));

        Assert.Empty(await GroupNamesAsync());
    }

    /// <summary>Saves one group, then a second one that needs the first id, then ends with <paramref name="end"/>.</summary>
    private async Task<Result<int>> RunAsync(Func<int, Result<int>> end)
    {
        await using var db = postgres.CreateContext();
        return await new CatalogTransaction(db).RunAsync(async ct =>
        {
            var first = Group("Перша", "persha");
            db.ProductGroups.Add(first);
            await db.SaveChangesAsync(ct);
            db.ProductGroups.Add(Group("Друга", $"druha-{first.Id.Value}"));
            await db.SaveChangesAsync(ct);
            return end(first.Id.Value);
        }, CancellationToken.None);
    }

    private static ProductGroup Group(string name, string slug)
        => ProductGroup.CreateManual(ProductCategoryId.Create(CategoryId), name, slug, DateTimeOffset.UtcNow).Value;

    private async Task<List<string>> GroupNamesAsync()
    {
        await using var db = postgres.CreateContext();
        return await db.ProductGroups.AsNoTracking().OrderBy(group => group.Id).Select(group => group.Name)
            .ToListAsync();
    }

    private async Task SeedCategoryAsync()
    {
        await using var seed = postgres.CreateContext();
        await seed.Database.ExecuteSqlRawAsync(
            """TRUNCATE "CatalogProductGroups", "CategoryFaqItems", "CategoryContents", "ProductCategories" CASCADE;""");
        seed.Categories.Add(ProductCategory.Create(ProductCategoryId.Create(CategoryId), "000005530", "Ручки", "ruchky",
            CategoryPath.Create("n30")).Value);
        await seed.SaveChangesAsync();
    }
}
