using Content.Application.Features.SeoSettings.Common;

namespace Content.Application.Features.SeoSettings.Get;

/// <summary>The one settings row; the site and the admin read the same values.</summary>
public sealed record GetSiteSeoSettingsQuery : IQuery<Result<SiteSeoSettingsDetails>>;
