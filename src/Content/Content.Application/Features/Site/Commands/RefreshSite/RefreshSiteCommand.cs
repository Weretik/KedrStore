using Content.Application.Abstractions.Integrations;

namespace Content.Application.Features.Site.Commands.RefreshSite;

/// <summary>«Оновити сайт зараз»: the site shows the latest content on the next visit instead of within minutes.</summary>
public sealed record RefreshSiteCommand : ICommand<Result<SiteRefreshOutcome>>;
