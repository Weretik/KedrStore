using BuildingBlocks.Domain.Identifiers;
using Catalog.Application.Features.Groups.Queries.GetProductGroups;
using Catalog.Contracts.Groups;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Catalog.Persistence;

/// <summary>Database rules 2.1: the group picker matches every word of the search, in any order and case.</summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class ProductGroupSearchPostgresTests(CatalogPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Search_MatchesEveryWordOfTheName()
    {
        postgres.SkipIfUnavailable();
        await using (var seed = postgres.CreateContext())
        {
            await seed.Database.ExecuteSqlRawAsync(
                """TRUNCATE "CatalogProductGroups", "ProductCategories" CASCADE;""");
            seed.Categories.Add(ProductCategory.Create(ProductCategoryId.Create(10), "000005513", "Ручки", "ruchky",
                CategoryPath.Create("n10")).Value);
            var now = DateTimeOffset.UtcNow;
            foreach (var (name, slug) in new[]
                     { ("Ручка Кедр Модерн", "a"), ("Ручка Модерн", "b"), ("Кедр 50% Класик", "c") })
                seed.ProductGroups.Add(ProductGroup.CreateManual(ProductCategoryId.Create(10), name, slug, now).Value);
            await seed.SaveChangesAsync();
        }

        await using var db = postgres.CreateContext();
        var handler = new GetProductGroupsHandler(db);
        async Task<string[]> FindAsync(string search) =>
            (await handler.Handle(new GetProductGroupsQuery(new GetAdminProductGroupsRequest { Search = search }),
                CancellationToken.None)).Value.Select(group => group.Name).Order().ToArray();

        Assert.Equal(["Ручка Кедр Модерн"], await FindAsync("модерн КЕДР"));
        Assert.Equal(["Кедр 50% Класик"], await FindAsync("50%"));
    }
}
