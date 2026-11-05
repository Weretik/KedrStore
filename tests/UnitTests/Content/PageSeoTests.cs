using BuildingBlocks.Domain.Exceptions;
using Content.Domain.Seo;

namespace UnitTests.Content;

public sealed class PageSeoTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_TrimsValues_AndTurnsBlankIntoTheSiteDefault()
    {
        var seo = PageSeo.Create(SitePages.Contacts, new PageSeoValues("  Контакти  ", " ", null, "Описание"), Now,
            " Олена ");

        Assert.Equal("Контакти", seo.Title("uk"));
        Assert.Null(seo.Title("ru"));
        Assert.Null(seo.Description("uk"));
        Assert.Equal("Описание", seo.Description("ru"));
        Assert.Equal("Олена", seo.UpdatedBy);
    }

    [Fact]
    public void Create_RefusesAPageTheSiteDoesNotHave()
    {
        Assert.Throws<DomainException>(() =>
            PageSeo.Create("Checkout", new PageSeoValues("x", null, null, null), Now, "editor"));
    }

    [Fact]
    public void Update_RefusesATooLongTitle()
    {
        var seo = PageSeo.Create(SitePages.Home, new PageSeoValues(null, null, null, null), Now, "editor");

        Assert.Throws<DomainException>(() => seo.Update(
            new PageSeoValues(new string('a', PageSeo.TitleMaxLength + 1), null, null, null), Now, "editor"));
    }
}
