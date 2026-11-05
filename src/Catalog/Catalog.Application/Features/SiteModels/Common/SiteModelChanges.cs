using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Groups.Common;
using Catalog.Application.Groups;
using Catalog.Domain.Entities.Groups;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.SiteModels.Common;

/// <summary>
/// Shared steps of the model commands: checking the products a person adds (R-04), pinning them, saving the offered
/// axes of a model that has none (R-05), and the build that tidies proposed models afterwards.
/// </summary>
public sealed class SiteModelChanges(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroupAxis> axes,
    ProductGroupPins pins,
    ProductGroupBuilder builder,
    SiteModelStateReader stateReader)
{
    /// <summary>An error when a product is missing, of another category, or in another accepted model.</summary>
    public async Task<Result?> CheckProductsAsync(ProductCategoryId categoryId, IReadOnlyCollection<ProductId> productIds,
        ProductGroupId? modelId, CancellationToken cancellationToken)
    {
        var inCategory = await db.Products.IgnoreQueryFilters()
            .Where(product => productIds.Contains(product.Id) && product.CategoryId == categoryId)
            .Select(product => product.Id)
            .ToListAsync(cancellationToken);
        var wrong = productIds.Except(inCategory).Select(id => id.Value).ToList();
        if (wrong.Count > 0)
        {
            return SiteModelValidation.Invalid("productIds",
                $"Products not found in the model's category: {string.Join(", ", wrong)}.", "ProductNotInCategory");
        }

        var taken = await db.ProductGroupMembers
            .Where(member => productIds.Contains(member.ProductId) && member.GroupId != null && member.GroupId != modelId)
            .Join(db.ProductGroups.Where(group => group.Status == Domain.Enums.ProductGroupStatus.Accepted),
                member => member.GroupId, group => (ProductGroupId?)group.Id,
                (member, group) => new { member.ProductId, group.Name })
            .ToListAsync(cancellationToken);
        if (taken.Count > 0)
        {
            return SiteModelValidation.Invalid("productIds",
                "Products already in another model: " +
                string.Join(", ", taken.Select(row => $"{row.ProductId.Value} («{row.Name}»)")) + ".",
                "ProductInAnotherModel");
        }

        return null;
    }

    /// <summary>A model lives in a final category: one that exists and has no subcategories (M-03).</summary>
    public async Task<bool> IsFinalCategoryAsync(ProductCategoryId categoryId, CancellationToken cancellationToken)
        => await db.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken) &&
           !await db.Categories.AnyAsync(category => category.ParentId == categoryId, cancellationToken);

    public Task PinAsync(IReadOnlyCollection<ProductId> productIds, ProductGroupId? modelId, DateTimeOffset now,
        CancellationToken cancellationToken)
        => pins.PinAsync(productIds, modelId, now, cancellationToken);

    /// <summary>Saves the offered axes of each model that has none saved (R-05). Saves.</summary>
    public async Task SaveOfferedAxesAsync(IReadOnlyCollection<ProductGroupId> modelIds,
        CancellationToken cancellationToken)
    {
        var state = await stateReader.ReadAsync(cancellationToken);
        var created = modelIds.Where(id => state.Models.ContainsKey(id) && !state.SavedAxes[id].Any())
            .SelectMany(id => state.OfferedAxes(id).Select((attribute, index) =>
                ProductGroupAxis.Create(id, attribute.Id, index)))
            .ToList();
        if (created.Count == 0)
            return;

        await axes.AddRangeAsync(created, cancellationToken);
        await axes.SaveChangesAsync(cancellationToken);
    }

    public Task TidyAsync(CancellationToken cancellationToken) => builder.RunAsync(apply: true, cancellationToken);
}
