using Content.Application.Features.SiteSeo.Common;

namespace Content.Application.Features.SiteSeo.Admin.Update;

/// <summary>Replaces the four texts of one page; an empty value returns that text to the site default.</summary>
public sealed record UpdatePageSeoCommand(
    string Page,
    string? TitleUk,
    string? TitleRu,
    string? DescriptionUk,
    string? DescriptionRu) : ICommand<Result<AdminPageSeo>>;
