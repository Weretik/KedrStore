namespace Catalog.Application.Abstractions.Persistence;

/// <summary>Ids of categories made by hand, from their own database sequence outside 1C codes (spec catalog/012 MC-01).</summary>
public interface IManualCategoryIds
{
    Task<ProductCategoryId> NextAsync(CancellationToken cancellationToken);
}
