using Content.Application.Features.SiteSeo.Shared;

namespace Content.Application.Features.SiteSeo.Commands.UpdatePageSeo;

/// <summary>Replaces the four texts of one page; an empty value returns that text to the site default.</summary>
public sealed record UpdatePageSeoCommand(
    string Page,
    string? TitleUk,
    string? TitleRu,
    string? DescriptionUk,
    string? DescriptionRu) : ICommand<Result<AdminPageSeo>>;
