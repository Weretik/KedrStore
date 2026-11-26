using Content.Domain.Redirects;

namespace UnitTests.Content.Redirects;

/// <summary>Spec content/013 R-02: how old and new addresses are stored.</summary>
public sealed class SiteRedirectPathTests
{
    [Theory]
    [InlineData("/region/kharkiv", "/region/kharkiv")]
    [InlineData("  /Uk/Catalog/Old/  ", "/uk/catalog/old")]
    [InlineData("/uk/product/old?utm=1#top", "/uk/product/old")]
    [InlineData("https://classkedr.com.ua/uk/stara-storinka?x=1", "/uk/stara-storinka")]
    [InlineData("/uk/%D1%80%D1%83%D1%87%D0%BA%D0%B8", "/uk/ручки")]
    public void From_IsStoredAsTheSiteLooksItUp(string input, string expected)
    {
        var result = SiteRedirectPath.NormalizeFrom(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("", "Content.SiteRedirect.from.Required")]
    [InlineData("old-page", "Content.SiteRedirect.from.NotAPath")]
    [InlineData("//evil.example/x", "Content.SiteRedirect.from.NotAPath")]
    [InlineData("/old page", "Content.SiteRedirect.from.NotAPath")]
    [InlineData("/", "Content.SiteRedirect.from.Reserved")]
    [InlineData("/uk/", "Content.SiteRedirect.from.Reserved")]
    [InlineData("/api/content/contacts", "Content.SiteRedirect.from.Reserved")]
    [InlineData("/_next/static/x.js", "Content.SiteRedirect.from.Reserved")]
    public void From_RefusesWhatTheSiteCannotRedirect(string input, string code)
    {
        var result = SiteRedirectPath.NormalizeFrom(input);

        Assert.True(result.IsFailure);
        Assert.Equal(code, result.Error!.Code);
        Assert.Equal("from", result.Error.Field);
    }

    [Fact]
    public void From_RefusesAnOverlongPath() =>
        Assert.True(SiteRedirectPath.NormalizeFrom("/" + new string('a', SiteRedirectPath.FromMaxLength)).IsFailure);

    [Theory]
    [InlineData("/uk/catalog/ruchky/", "/uk/catalog/ruchky")]
    [InlineData("/uk/search?q=Ручка", "/uk/search?q=Ручка")]
    [InlineData("/", "/")]
    [InlineData(" https://partner.example/page?a=1 ", "https://partner.example/page?a=1")]
    public void To_KeepsAPathWithItsQueryOrAFullUrl(string input, string expected)
    {
        var result = SiteRedirectPath.NormalizeTo(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("catalog")]
    [InlineData("javascript:alert(1)")]
    [InlineData("//evil.example")]
    [InlineData("/a b")]
    public void To_RefusesWhatIsNotASitePathOrHttpUrl(string input) =>
        Assert.True(SiteRedirectPath.NormalizeTo(input).IsFailure);

    [Fact]
    public void KeyOfTarget_IsTheLookupKeyOfASitePath_AndNullForAFullUrl()
    {
        Assert.Equal("/uk/catalog/ruchky", SiteRedirectPath.KeyOfTarget("/uk/Catalog/Ruchky?page=2"));
        Assert.Null(SiteRedirectPath.KeyOfTarget("https://partner.example/page"));
    }
}
