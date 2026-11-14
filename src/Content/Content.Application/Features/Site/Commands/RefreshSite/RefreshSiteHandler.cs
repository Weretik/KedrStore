using Content.Application.Abstractions.Integrations;

namespace Content.Application.Features.Site.Commands.RefreshSite;

public sealed class RefreshSiteHandler(ISiteCacheRefresher refresher)
    : ICommandHandler<RefreshSiteCommand, Result<SiteRefreshOutcome>>
{
    public async ValueTask<Result<SiteRefreshOutcome>> Handle(RefreshSiteCommand command,
        CancellationToken cancellationToken)
        => Result.Success(await refresher.RefreshAsync(cancellationToken));
}
