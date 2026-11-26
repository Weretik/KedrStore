using Content.Domain.Redirects;

namespace UnitTests.Content.Redirects;

/// <summary>Spec content/013 R-01, R-02: creating and changing a redirect.</summary>
public sealed class SiteRedirectTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_StoresNormalizedAddressesAndTheEditor()
    {
        var result = SiteRedirect.Create(" /Region/Kyiv/ ", "/uk", Now, " seo@kedr ");

        Assert.True(result.IsSuccess);
        Assert.Equal("/region/kyiv", result.Value.FromPath);
        Assert.Equal("/uk", result.Value.ToUrl);
        Assert.Equal(Now, result.Value.CreatedAt);
        Assert.Equal(Now, result.Value.UpdatedAt);
        Assert.Equal("seo@kedr", result.Value.UpdatedBy);
    }

    [Fact]
    public void Create_RefusesARedirectToItself()
    {
        var result = SiteRedirect.Create("/uk/old", "/UK/old/?utm=1", Now, "seo");

        Assert.True(result.IsFailure);
        Assert.Equal("Content.SiteRedirect.To.Same", result.Error!.Code);
    }

    [Fact]
    public void Change_ReplacesBothAddresses_AndKeepsTheOldOnesOnFailure()
    {
        var redirect = SiteRedirect.Create("/uk/old", "/uk/new", Now, "seo").Value;

        var refused = redirect.Change("/uk/old", "", Now.AddHours(1), "admin");
        var changed = redirect.Change("/uk/older", "/uk/newer", Now.AddHours(2), "admin");

        Assert.True(refused.IsFailure);
        Assert.True(changed.IsSuccess);
        Assert.Equal("/uk/older", redirect.FromPath);
        Assert.Equal("/uk/newer", redirect.ToUrl);
        Assert.Equal(Now.AddHours(2), redirect.UpdatedAt);
        Assert.Equal(Now, redirect.CreatedAt);
    }
}
