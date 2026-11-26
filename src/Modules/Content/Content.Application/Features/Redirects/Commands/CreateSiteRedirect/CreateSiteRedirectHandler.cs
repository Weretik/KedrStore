using BuildingBlocks.Application.Users;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Shared;

namespace Content.Application.Features.Redirects.Commands.CreateSiteRedirect;

/// <summary>
/// Creates the redirect when its old address is free and it makes no chain; a racing copy with the same old address
/// breaks the unique index and answers 409.
/// </summary>
public sealed class CreateSiteRedirectHandler(
    IContentRepository<SiteRedirect> repository,
    IReadContentDbContext db,
    IContentUniqueSave save,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<CreateSiteRedirectCommand, Result<AdminSiteRedirect>>
{
    public async ValueTask<Result<AdminSiteRedirect>> Handle(CreateSiteRedirectCommand command,
        CancellationToken cancellationToken)
    {
        if (await db.SiteRedirects.CountAsync(cancellationToken) >= SiteRedirect.MaxCount)
            return SiteRedirectErrors.LimitReached(SiteRedirect.MaxCount).ToResult<AdminSiteRedirect>();

        var created = SiteRedirect.Create(command.From, command.To, clock.GetUtcNow(), ContentEditor.Name(currentUser));
        if (created.IsFailure)
            return created.Error!.ToResult<AdminSiteRedirect>();
        if (await SiteRedirectRules.CheckAsync(db, created.Value, cancellationToken) is { } error)
            return error.ToResult<AdminSiteRedirect>();

        await repository.AddAsync(created.Value, cancellationToken);
        if (!await save.TrySaveChangesAsync(cancellationToken))
            return SiteRedirectErrors.Duplicate().ToResult<AdminSiteRedirect>();
        return Result.Success(SiteRedirectMapper.ToAdmin(created.Value));
    }
}
