using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Groups.Shared;
using Catalog.Domain.Groups;
using Catalog.Domain.Groups.Members;
namespace Catalog.Application.Features.SiteModels.Shared.State;

/// <summary>
/// Shared steps of the model commands: checking the products a person adds (R-04, R-23), accepting models (R-03),
/// pinning products, saving the offered axes of a model that has none (R-05), and the build that tidies proposed
/// models afterwards.
/// </summary>
public sealed class SiteModelChanges(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> models,
    ProductGroupPins pins,
    ProductGroupBuilder builder,
    SiteModelStateReader stateReader)
{
    /// <summary>
    /// An error when a product is missing or in another accepted model. A product of any 1C category may join (R-23).
    /// </summary>
    public async Task<Result?> CheckProductsAsync(IReadOnlyCollection<ProductId> productIds, ProductGroupId? modelId,
        CancellationToken cancellationToken)
    {
        var found = await db.Products.IgnoreQueryFilters()
            .Where(product => productIds.Contains(product.Id))
            .Select(product => product.Id)
            .ToListAsync(cancellationToken);
        var missing = productIds.Except(found).Select(id => id.Value).ToList();
        if (missing.Count > 0)
        {
            return SiteModelValidation.Invalid("productIds",
                $"Products not found: {string.Join(", ", missing)}.", "ProductNotFound");
        }

        var taken = await db.ProductGroupMembers
            .Where(member => productIds.Contains(member.ProductId) && member.GroupId != null && member.GroupId != modelId)
            .Join(db.ProductGroups.Where(group => group.Status == Domain.Groups.ProductGroupStatus.Accepted),
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

    /// <summary>
    /// Accepts the proposed models among the given tracked ones: saves their offered axes first (R-05), then accepts
    /// them, saves, pins their products, and tidies (R-03). Returns the ids it accepted.
    /// </summary>
    public async Task<IReadOnlyList<ProductGroupId>> AcceptAsync(IReadOnlyCollection<ProductGroup> found, string acceptedBy,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        // Offered axes are read while the models are still proposed.
        await SaveOfferedAxesAsync(found.Where(model => !model.IsAccepted).Select(model => model.Id).ToList(),
            cancellationToken);

        var accepted = found.Where(model => model.Accept(acceptedBy, now)).Select(model => model.Id).ToList();
        if (accepted.Count == 0)
            return accepted;

        await models.SaveChangesAsync(cancellationToken);
        var members = await db.ProductGroupMembers
            .Where(member => member.GroupId != null && accepted.Contains(member.GroupId.Value))
            .Select(member => new { member.ProductId, GroupId = member.GroupId!.Value })
            .ToListAsync(cancellationToken);
        foreach (var model in members.GroupBy(member => member.GroupId))
            await PinAsync(model.Select(member => member.ProductId).ToList(), model.Key, now, cancellationToken);
        await TidyAsync(cancellationToken);
        return accepted;
    }

    public Task PinAsync(IReadOnlyCollection<ProductId> productIds, ProductGroupId? modelId, DateTimeOffset now,
        CancellationToken cancellationToken)
        => pins.PinAsync(productIds, modelId, now, cancellationToken);

    /// <summary>Saves the offered axes of each model that has none saved (R-05). Saves.</summary>
    public async Task SaveOfferedAxesAsync(IReadOnlyCollection<ProductGroupId> modelIds,
        CancellationToken cancellationToken)
    {
        var state = await stateReader.ReadModelsAsync(modelIds, cancellationToken);
        var withoutAxes = modelIds.Where(id => state.Models.ContainsKey(id) && !state.SavedAxes[id].Any()).ToList();
        if (withoutAxes.Count == 0)
            return;

        var changed = false;
        foreach (var model in await models.ListAsync(new ProductGroupsWithAxesSpec(withoutAxes), cancellationToken))
            changed |= model.Axes.Count == 0 && model.SetAxes(state.OfferedAxes(model.Id).Select(attribute => attribute.Id).ToList());
        if (changed)
            await models.SaveChangesAsync(cancellationToken);
    }

    public Task TidyAsync(CancellationToken cancellationToken) => builder.RunAsync(apply: true, cancellationToken);
}
