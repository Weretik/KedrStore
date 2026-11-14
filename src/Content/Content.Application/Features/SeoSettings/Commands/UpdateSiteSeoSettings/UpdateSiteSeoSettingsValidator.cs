namespace Content.Application.Features.SeoSettings.Commands.UpdateSiteSeoSettings;

/// <summary>Same rules as the domain, reported per field by its JSON name so the admin form can show them.</summary>
public sealed class UpdateSiteSeoSettingsValidator : AbstractValidator<UpdateSiteSeoSettingsCommand>
{
    private const string VerificationMessage = "Paste the code from the console or its whole meta tag.";

    public UpdateSiteSeoSettingsValidator()
    {
        RuleFor(x => x.Input).NotNull().OverridePropertyName("body");
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.GoogleAnalyticsId)
                .Must(value => SiteSeoSettings.NormalizeId(value) is not { } id || SiteSeoSettings.IsAnalyticsId(id))
                .WithMessage("Use a measurement id like G-AB12CD34EF.").OverridePropertyName("googleAnalyticsId");
            RuleFor(x => x.Input.GoogleTagManagerId)
                .Must(value => SiteSeoSettings.NormalizeId(value) is not { } id || SiteSeoSettings.IsTagManagerId(id))
                .WithMessage("Use a container id like GTM-AB12CD3.").OverridePropertyName("googleTagManagerId");
            RuleFor(x => x.Input.GoogleSiteVerification).Must(IsVerificationOrBlank)
                .WithMessage(VerificationMessage).OverridePropertyName("googleSiteVerification");
            RuleFor(x => x.Input.BingSiteVerification).Must(IsVerificationOrBlank)
                .WithMessage(VerificationMessage).OverridePropertyName("bingSiteVerification");
            RuleFor(x => x.Input.ShareImageMediaFileId).GreaterThan(0)
                .When(x => x.Input.ShareImageMediaFileId is not null).OverridePropertyName("shareImageMediaFileId");
        });
    }

    private static bool IsVerificationOrBlank(string? value) =>
        SiteSeoSettings.NormalizeVerification(value) is not { } code || SiteSeoSettings.IsVerification(code);
}
