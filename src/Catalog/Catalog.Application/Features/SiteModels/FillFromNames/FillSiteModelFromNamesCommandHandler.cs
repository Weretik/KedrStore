using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.SiteModels.Common;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.SiteModels.FillFromNames;

/// <summary>
/// «Заповнити з назв» (R-06): for each product of the model and each axis a person has not set, the confirmed values the
/// recognizer stored become manual values. Codes nobody decoded have no confirmed value and stay empty
/// (spec catalog/006 D-03).
/// </summary>
public sealed class FillSiteModelFromNamesCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductAttributeValue> values,
    SiteModelStateReader stateReader,
    SiteModelChanges changes,
    SiteModelReader reader,
    TimeProvider clock)
    : ICommandHandler<FillSiteModelFromNamesCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(FillSiteModelFromNamesCommand command,
        CancellationToken cancellationToken)
    {
        var modelId = ProductGroupId.Create(command.ModelId);
        var state = await stateReader.ReadAsync(cancellationToken);
        if (!state.Models.ContainsKey(modelId))
            return Result.NotFound();

        var axisIds = state.AxesOf(modelId).Select(axis => axis.Attribute.Id).ToList();
        var productIds = state.MembersByModel[modelId].Where(id => !state.Products[id].IsDeleted).ToList();
        var rows = await db.ProductAttributeValues
            .Where(value => productIds.Contains(value.ProductId) && axisIds.Contains(value.AttributeId))
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow();
        var changed = false;
        foreach (var cell in rows.GroupBy(row => (row.ProductId, row.AttributeId)))
        {
            if (cell.Any(row => !row.IsAutomatic) || cell.Any(row => !row.IsConfirmed))
                continue;

            foreach (var row in cell)
                changed |= row.SetManually(row.ValueId, row.NumberValue, now);
        }

        if (changed)
        {
            await values.SaveChangesAsync(cancellationToken);
            await changes.TidyAsync(cancellationToken);
        }

        return Result.Success((await reader.ReadAsync(command.ModelId, cancellationToken))!);
    }
}
