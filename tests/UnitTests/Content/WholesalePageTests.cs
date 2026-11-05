using BuildingBlocks.Domain.Exceptions;
using Content.Domain.Wholesale;

namespace UnitTests.Content;

public sealed class WholesalePageTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);

    internal static WholesalePageTextValues Values(string prefix = "Текст", int faqCount = 2) => new(
        $"{prefix} hero", "Lead", "Card", "Note", "Assortment", "Intro", "company title", "Company text",
        "Selection", "selection text", "Warehouse", "warehouse text", "Steps",
        [("Крок 1", "Опис 1"), ("Крок 2", "Опис 2"), ("Крок 3", "Опис 3")],
        "Faq", Enumerable.Range(1, faqCount).Select(i => ($"Питання {i}?", $"Відповідь {i}.")).ToList(),
        "Closing");

    [Fact]
    public void Create_TrimsTexts_AndKeepsStepsAndQuestionsInOrder()
    {
        var text = WholesalePageText.Create(Values() with { HeroTitle = "  Оптом  " });

        Assert.Equal("Оптом", text.HeroTitle);
        Assert.Equal(["Крок 1", "Крок 2", "Крок 3"], text.Steps.Select(step => step.Title));
        Assert.Equal("Відповідь 2.", text.Faq[1].Answer);
    }

    [Fact]
    public void Create_RefusesAnEmptyText_WrongStepCount_AndTooManyQuestions()
    {
        Assert.Throws<DomainException>(() => WholesalePageText.Create(Values() with { CompanyText = " " }));
        Assert.Throws<DomainException>(() => WholesalePageText.Create(Values() with { Steps = [("a", "b")] }));
        Assert.Throws<DomainException>(() => WholesalePageText.Create(Values(faqCount: 0)));
        Assert.Throws<DomainException>(() => WholesalePageText.Create(Values(faqCount: WholesalePageText.MaxFaqItems + 1)));
        Assert.Throws<DomainException>(() => WholesalePageText.Create(
            Values() with { HeroTitle = new string('x', WholesalePageText.TitleMaxLength + 1) }));
    }

    [Fact]
    public void Update_ReplacesBothLanguages_AndRecordsTheEditor()
    {
        var page = WholesalePage.Create(WholesalePageText.Create(Values("uk")), WholesalePageText.Create(Values("ru")), Now);
        Assert.Null(page.UpdatedBy);

        page.Update(WholesalePageText.Create(Values("нове")), WholesalePageText.Create(Values("новое")),
            Now.AddHours(1), " manager@classkedr.com.ua ");

        Assert.Equal("нове hero", page.Text("uk").HeroTitle);
        Assert.Equal("новое hero", page.Text("ru").HeroTitle);
        Assert.Equal("manager@classkedr.com.ua", page.UpdatedBy);
        Assert.Throws<DomainException>(() => page.Text("en"));
    }
}
