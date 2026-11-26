namespace Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;

/// <summary>Removes a redirect; the old address answers as the site would without it.</summary>
public sealed record DeleteSiteRedirectCommand(long Id) : ICommand<Result>;
