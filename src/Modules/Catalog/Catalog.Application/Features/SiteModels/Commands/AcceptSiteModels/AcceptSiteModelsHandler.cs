using BuildingBlocks.Application.Users;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Application.Features.SiteModels.Shared.State;

namespace Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;

/// <summary>
/// «Прийняти»: the models become accepted, their products are pinned, and the offered axes are saved (R-03, R-05).
/// A model that is already accepted is left as it is.
/// </summary>
public sealed class AcceptSiteModelsHandler(
    IReadCatalogDbContext db,
    ICurrentUserService currentUser,
    SiteModelChanges changes,
    SiteModelStateReader stateReader,
    TimeProvider clock,
    ICatalogTransaction transaction)
    : ICommandHandler<AcceptSiteModelsCommand, Result<SiteModelHealthDto>>
{
    public async ValueTask<Result<SiteModelHealthDto>> Handle(AcceptSiteModelsCommand command, CancellationToken cancellationToken)
        => await transaction.RunAsync(ct => HandleInTransactionAsync(command, ct), cancellationToken);

    private async Task<Result<SiteModelHealthDto>> HandleInTransactionAsync(AcceptSiteModelsCommand command, CancellationToken cancellationToken)
    {
        var ids = command.Request.Ids.Select(ProductGroupId.Create).ToList();
        var found = await db.ProductGroups.Where(group => ids.Contains(group.Id)).ToListAsync(cancellationToken);
        var missing = ids.Except(found.Select(group => group.Id)).Select(id => id.Value).ToList();
        if (missing.Count > 0)
            return SiteModelValidation.Invalid("ids", $"Моделей не знайдено: {string.Join(", ", missing)}.", "NotFound");

        await changes.AcceptAsync(found, ReviewerName.Of(currentUser), clock.GetUtcNow(), cancellationToken);
        return Result.Success((await stateReader.ReadFreshCatalogAsync(cancellationToken)).Health());
    }
}
