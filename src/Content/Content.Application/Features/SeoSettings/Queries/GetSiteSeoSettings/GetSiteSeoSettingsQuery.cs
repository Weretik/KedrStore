using Content.Application.Features.SeoSettings.Shared;

namespace Content.Application.Features.SeoSettings.Queries.GetSiteSeoSettings;

/// <summary>The one settings row; the site and the admin read the same values.</summary>
public sealed record GetSiteSeoSettingsQuery : IQuery<Result<SiteSeoSettingsDetails>>;
