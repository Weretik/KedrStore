using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.CreateSiteRedirect;

/// <summary>Adds a permanent redirect from an old site address to a new one.</summary>
public sealed record CreateSiteRedirectCommand(string? From, string? To) : ICommand<Result<AdminSiteRedirect>>;
