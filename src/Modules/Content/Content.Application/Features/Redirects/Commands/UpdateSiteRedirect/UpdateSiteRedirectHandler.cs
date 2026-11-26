using BuildingBlocks.Application.Users;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Shared;

namespace Content.Application.Features.Redirects.Commands.UpdateSiteRedirect;

public sealed class UpdateSiteRedirectHandler(
    IContentRepository<SiteRedirect> repository,
    IReadContentDbContext db,
    IContentUniqueSave save,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateSiteRedirectCommand, Result<AdminSiteRedirect>>
{
    public async ValueTask<Result<AdminSiteRedirect>> Handle(UpdateSiteRedirectCommand command,
        CancellationToken cancellationToken)
    {
        var redirect = await repository.GetByIdAsync(SiteRedirectId.FromStorage(command.Id), cancellationToken);
        if (redirect is null)
            return Result.NotFound();
        if (RowVersions.IsStale(command.Version, redirect.Version))
            return RowVersions.Changed<AdminSiteRedirect>();

        var changed = redirect.Change(command.From, command.To, clock.GetUtcNow(), ContentEditor.Name(currentUser));
        if (changed.IsFailure)
            return changed.Error!.ToResult<AdminSiteRedirect>();
        if (await SiteRedirectRules.CheckAsync(db, redirect, cancellationToken) is { } error)
            return error.ToResult<AdminSiteRedirect>();

        if (!await save.TrySaveChangesAsync(cancellationToken))
            return SiteRedirectErrors.Duplicate().ToResult<AdminSiteRedirect>();
        return Result.Success(SiteRedirectMapper.ToAdmin(redirect));
    }
}
