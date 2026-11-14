using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Commands.RemoveSiteModelProduct;

/// <summary>
/// Takes a product out of the model and pins it out of models (R-04). An on-site product left without a model is the
/// «no_model» error until a person puts it into another one. Returns the model, or 404 once the build removed an
/// emptied proposed model.
/// </summary>
public sealed class RemoveSiteModelProductHandler(
    IReadCatalogDbContext db,
    SiteModelChanges changes,
    SiteModelReader reader,
    TimeProvider clock)
    : ICommandHandler<RemoveSiteModelProductCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(RemoveSiteModelProductCommand command,
        CancellationToken cancellationToken)
    {
        var modelId = ProductGroupId.Create(command.ModelId);
        var productId = ProductId.From(command.ProductId);
        if (!await db.ProductGroupMembers.AnyAsync(member => member.ProductId == productId && member.GroupId == modelId,
                cancellationToken))
            return Result.NotFound();

        await changes.PinAsync([productId], null, clock.GetUtcNow(), cancellationToken);
        await changes.TidyAsync(cancellationToken);

        return await reader.ReadAsync(command.ModelId, cancellationToken) is { } model
            ? Result.Success(model)
            : Result.NotFound();
    }
}
