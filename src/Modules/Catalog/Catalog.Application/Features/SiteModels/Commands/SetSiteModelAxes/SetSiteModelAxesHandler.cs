using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups.Members;
using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Attributes.Enums;

namespace Catalog.Application.Features.SiteModels.Commands.SetSiteModelAxes;

/// <summary>Replaces the model's axes with the listed attributes, in that order (R-05). Number attributes are refused.</summary>
public sealed class SetSiteModelAxesHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> models,
    SiteModelReader reader)
    : ICommandHandler<SetSiteModelAxesCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(SetSiteModelAxesCommand command,
        CancellationToken cancellationToken)
    {
        var modelId = ProductGroupId.Create(command.ModelId);
        var model = await models.FirstOrDefaultAsync(new ProductGroupsWithAxesSpec([modelId]), cancellationToken);
        if (model is null)
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

        if (model.SetAxes(attributeIds))
            await models.SaveChangesAsync(cancellationToken);

        return Result.Success((await reader.ReadAsync(command.ModelId, cancellationToken))!);
    }
}
