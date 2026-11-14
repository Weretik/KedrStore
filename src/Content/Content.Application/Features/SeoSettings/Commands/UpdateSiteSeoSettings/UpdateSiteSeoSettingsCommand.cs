using Content.Application.Features.SeoSettings.Shared;

namespace Content.Application.Features.SeoSettings.Commands.UpdateSiteSeoSettings;

public sealed record UpdateSiteSeoSettingsCommand(SiteSeoSettingsInput Input)
    : ICommand<Result<SiteSeoSettingsDetails>>;
