using BuildingBlocks.Application.Contracts;
using Content.Application.Shared;
using Content.Application.Features.Wholesale.Shared;

namespace Content.Application.Features.Wholesale.Commands.UpdateWholesalePage;

/// <summary>The row comes from the migration, so a missing one is 404.</summary>
public sealed class UpdateWholesalePageHandler(
    IContentRepository<WholesalePage> repository,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateWholesalePageCommand, Result<AdminWholesalePage>>
{
    public async ValueTask<Result<AdminWholesalePage>> Handle(UpdateWholesalePageCommand command,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetByIdAsync(WholesalePage.SingletonId, cancellationToken);
        if (page is null)
            return Result.NotFound();

        var texts = command.Texts.ToDictionary(text => text.Language);
        var uk = WholesaleMapper.ToText(texts["uk"]);
        if (uk.IsFailure)
            return uk.Error!.ToResult<AdminWholesalePage>();
        var ru = WholesaleMapper.ToText(texts["ru"]);
        if (ru.IsFailure)
            return ru.Error!.ToResult<AdminWholesalePage>();
        page.Update(uk.Value, ru.Value, clock.GetUtcNow(), ContentEditor.Name(currentUser));
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(WholesaleMapper.ToAdmin(page));
    }
}
