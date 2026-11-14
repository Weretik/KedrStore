using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.SiteModels.Common;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.SetAxes;

/// <summary>Replaces the model's axes with the listed attributes, in that order (R-05). Number attributes are refused.</summary>
public sealed class SetSiteModelAxesCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroupAxis> axes,
    SiteModelReader reader)
    : ICommandHandler<SetSiteModelAxesCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(SetSiteModelAxesCommand command,
        CancellationToken cancellationToken)
    {
        var modelId = ProductGroupId.Create(command.ModelId);
        if (!await db.ProductGroups.AnyAsync(group => group.Id == modelId, cancellationToken))
            return Result.NotFound();

        var attributeIds = command.Request.AttributeIds.Select(AttributeId.Create).ToList();
        var known = await db.Attributes
            .Where(attribute => attributeIds.Contains(attribute.Id) && attribute.Kind != AttributeKind.Number)
            .Select(attribute => attribute.Id)
            .ToListAsync(cancellationToken);
        var wrong = attributeIds.Except(known).Select(id => id.Value).ToList();
        if (wrong.Count > 0)
        {
            return SiteModelValidation.Invalid("attributeIds",
                $"Attributes not found or numeric: {string.Join(", ", wrong)}.", "AttributeNotFound");
        }

        var existing = await db.ProductGroupAxes.Where(axis => axis.GroupId == modelId).ToListAsync(cancellationToken);
        var removed = existing.Where(axis => !attributeIds.Contains(axis.AttributeId)).ToList();
        foreach (var axis in existing.Except(removed))
            axis.MoveTo(attributeIds.IndexOf(axis.AttributeId));
        var created = attributeIds.Select((id, index) => (id, index))
            .Where(item => existing.All(axis => axis.AttributeId != item.id))
            .Select(item => ProductGroupAxis.Create(modelId, item.id, item.index))
            .ToList();

        if (removed.Count > 0)
            await axes.DeleteRangeAsync(removed, cancellationToken);
        if (created.Count > 0)
            await axes.AddRangeAsync(created, cancellationToken);
        await axes.SaveChangesAsync(cancellationToken);

        return Result.Success((await reader.ReadAsync(command.ModelId, cancellationToken))!);
    }
}
