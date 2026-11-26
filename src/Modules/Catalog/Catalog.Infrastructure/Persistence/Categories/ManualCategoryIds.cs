using Catalog.Application.Abstractions.Persistence;

namespace Catalog.Infrastructure.Persistence.Categories;

/// <summary>The next id of a category made by hand, from <c>catalog.ManualCategoryIds</c> (spec catalog/011 MC-01).</summary>
public sealed class ManualCategoryIds(CatalogDbContext db) : IManualCategoryIds
{
    public const string SequenceName = "ManualCategoryIds";
    public const int FirstId = 800_000_000;

    public async Task<ProductCategoryId> NextAsync(CancellationToken cancellationToken)
    {
        var next = await db.Database
            .SqlQueryRaw<int>($"SELECT nextval('{CatalogDbContext.Schema}.\"{SequenceName}\"')::int AS \"Value\"")
            .SingleAsync(cancellationToken);
        return ProductCategoryId.Create(next);
    }
}
