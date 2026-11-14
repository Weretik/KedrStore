using UnitTests.BuildingBlocks.Results;
using Content.Application.Features.SeoSettings.Shared;
using Content.Application.Features.SeoSettings.Commands.UpdateSiteSeoSettings;
using Content.Domain.Seo;

namespace UnitTests.Content;

public sealed class SiteSeoSettingsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Update_NormalizesIdsAndTakesTheCodeOutOfAPastedMetaTag()
    {
        var settings = SiteSeoSettings.CreateEmpty(Now);

        settings.Update(new SiteSeoSettingsValues(" g-ab12cd34ef ", "gtm-ab12cd3",
            """<meta name="google-site-verification" content="AbCdEf_123-xyz" />""",
            "<meta name='msvalidate.01' content='0123456789ABCDEF0123456789ABCDEF' />", 42), Now, "seo@kedr");

        Assert.Equal(SiteSeoSettings.SingletonId, settings.Id);
        Assert.Equal("G-AB12CD34EF", settings.GoogleAnalyticsId);
        Assert.Equal("GTM-AB12CD3", settings.GoogleTagManagerId);
        Assert.Equal("AbCdEf_123-xyz", settings.GoogleSiteVerification);
        Assert.Equal("0123456789ABCDEF0123456789ABCDEF", settings.BingSiteVerification);
        Assert.Equal(42, settings.ShareImageMediaFileId);
        Assert.Equal("seo@kedr", settings.UpdatedBy);
    }

    [Fact]
    public void Update_BlankValuesRemoveTheCodes()
    {
        var settings = SiteSeoSettings.CreateEmpty(Now);
        settings.Update(new SiteSeoSettingsValues("G-AB12CD34EF", "GTM-AB12CD3", "AbCdEf_123", "BingCode123", 7),
            Now, "seo");

        settings.Update(new SiteSeoSettingsValues(" ", null, "", null, null), Now, "seo");

        Assert.Null(settings.GoogleAnalyticsId);
        Assert.Null(settings.GoogleTagManagerId);
        Assert.Null(settings.GoogleSiteVerification);
        Assert.Null(settings.BingSiteVerification);
        Assert.Null(settings.ShareImageMediaFileId);
    }

    [Theory]
    [InlineData("UA-12345-1")]
    [InlineData("G-AB")]
    [InlineData("<script>")]
    public void Update_RejectsAnotherAnalyticsId(string id)
    {
        var settings = SiteSeoSettings.CreateEmpty(Now);

        var error = DomainAssert.Fails(
            settings.Update(new SiteSeoSettingsValues(id, null, null, null, null), Now, "seo"));
        Assert.Equal("GoogleAnalyticsId", error.Field);
    }

    [Fact]
    public void Validator_ReportsEachInvalidFieldByItsJsonName()
    {
        var input = new SiteSeoSettingsInput("GA-1", "GTM-1", "short", "has space in it", 0);

        var result = new UpdateSiteSeoSettingsValidator().Validate(new UpdateSiteSeoSettingsCommand(input));

        Assert.Equal(["bingSiteVerification", "googleAnalyticsId", "googleSiteVerification", "googleTagManagerId",
                "shareImageMediaFileId"],
            result.Errors.Select(e => e.PropertyName).Distinct().Order());
    }

    [Fact]
    public void Validator_AcceptsEmptySettings()
    {
        var input = new SiteSeoSettingsInput(null, "", " ", null, null);

        Assert.True(new UpdateSiteSeoSettingsValidator().Validate(new UpdateSiteSeoSettingsCommand(input)).IsValid);
    }
}
