using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Commands.RenameSiteModel;

/// <summary>Renames a model; its slug stays (spec catalog/006 R-12).</summary>
public sealed class RenameSiteModelHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> models,
    SiteModelReader reader,
    TimeProvider clock)
    : ICommandHandler<RenameSiteModelCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(RenameSiteModelCommand command,
        CancellationToken cancellationToken)
    {
        var id = ProductGroupId.Create(command.ModelId);
        var model = await db.ProductGroups.SingleOrDefaultAsync(group => group.Id == id, cancellationToken);
        if (model is null)
            return Result.NotFound();

        var renamed = model.Rename(command.Request.Name, clock.GetUtcNow());
        if (renamed.IsFailure)
            return renamed.Error!.ToResult<SiteModelDto>();
        if (renamed.Value)
            await models.SaveChangesAsync(cancellationToken);

        return Result.Success((await reader.ReadAsync(command.ModelId, cancellationToken))!);
    }
}
