using Content.Application.Features.SeoSettings.Common;

namespace Content.Application.Features.SeoSettings.Update;

public sealed record UpdateSiteSeoSettingsCommand(SiteSeoSettingsInput Input)
    : ICommand<Result<SiteSeoSettingsDetails>>;
