using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.UpdateSiteRedirect;

/// <summary>Replaces both addresses of a redirect; <c>Version</c> from the admin's read refuses a save over newer data.</summary>
public sealed record UpdateSiteRedirectCommand(long Id, string? From, string? To, uint? Version = null)
    : ICommand<Result<AdminSiteRedirect>>;
