using BuildingBlocks.Application.Users;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;

/// <summary>
/// «Прийняти»: the models become accepted, their products are pinned, and the offered axes are saved (R-03, R-05).
/// A model that is already accepted is left as it is.
/// </summary>
public sealed class AcceptSiteModelsHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> models,
    ICurrentUserService currentUser,
    SiteModelChanges changes,
    SiteModelStateReader stateReader,
    TimeProvider clock)
    : ICommandHandler<AcceptSiteModelsCommand, Result<SiteModelHealthDto>>
{
    public async ValueTask<Result<SiteModelHealthDto>> Handle(AcceptSiteModelsCommand command,
        CancellationToken cancellationToken)
    {
        var ids = command.Request.Ids.Select(ProductGroupId.Create).ToList();
        var found = await db.ProductGroups.Where(group => ids.Contains(group.Id)).ToListAsync(cancellationToken);
        var missing = ids.Except(found.Select(group => group.Id)).Select(id => id.Value).ToList();
        if (missing.Count > 0)
            return SiteModelValidation.Invalid("ids", $"Моделей не знайдено: {string.Join(", ", missing)}.", "NotFound");

        // Offered axes are read while the models are still proposed.
        await changes.SaveOfferedAxesAsync(found.Where(model => !model.IsAccepted).Select(model => model.Id).ToList(),
            cancellationToken);

        var now = clock.GetUtcNow();
        var by = ReviewerName.Of(currentUser);
        var accepted = found.Where(model => model.Accept(by, now)).Select(model => model.Id).ToList();
        if (accepted.Count == 0)
            return Result.Success((await stateReader.ReadAsync(cancellationToken)).Health());

        await models.SaveChangesAsync(cancellationToken);
        var members = await db.ProductGroupMembers
            .Where(member => member.GroupId != null && accepted.Contains(member.GroupId.Value))
            .Select(member => new { member.ProductId, GroupId = member.GroupId!.Value })
            .ToListAsync(cancellationToken);
        foreach (var model in members.GroupBy(member => member.GroupId))
            await changes.PinAsync(model.Select(member => member.ProductId).ToList(), model.Key, now, cancellationToken);
        await changes.TidyAsync(cancellationToken);

        return Result.Success((await stateReader.ReadAsync(cancellationToken)).Health());
    }
}
