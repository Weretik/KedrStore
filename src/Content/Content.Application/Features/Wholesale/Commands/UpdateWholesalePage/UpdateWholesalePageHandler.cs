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
        page.Update(WholesaleMapper.ToText(texts["uk"]), WholesaleMapper.ToText(texts["ru"]),
            clock.GetUtcNow(), ContentEditor.Name(currentUser));
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(WholesaleMapper.ToAdmin(page));
    }
}
