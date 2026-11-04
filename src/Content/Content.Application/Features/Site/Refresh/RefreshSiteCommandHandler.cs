using Content.Application.Contracts.Site;

namespace Content.Application.Features.Site.Refresh;

public sealed class RefreshSiteCommandHandler(ISiteCacheRefresher refresher)
    : ICommandHandler<RefreshSiteCommand, Result<SiteRefreshOutcome>>
{
    public async ValueTask<Result<SiteRefreshOutcome>> Handle(RefreshSiteCommand command,
        CancellationToken cancellationToken)
        => Result.Success(await refresher.RefreshAsync(cancellationToken));
}
