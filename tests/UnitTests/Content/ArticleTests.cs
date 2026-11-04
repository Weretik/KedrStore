using BuildingBlocks.Domain.Exceptions;
using Content.Application.Features.Articles.Admin.Create;
using Content.Application.Features.Articles.Admin.Update;
using Content.Application.Features.Articles.Common;
using Content.Domain.Articles;

namespace UnitTests.Content;

public sealed class ArticleTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Day = new(2026, 11, 4);

    private static ArticleText[] Texts(string ukTitle = "Петлі") =>
    [
        new("ru", "Петли", "Коротко.", "<p>Текст</p>"),
        new("uk", ukTitle, "Коротко.", "<p>Текст</p>"),
    ];

    [Fact]
    public void Create_KeepsBothLanguagesSortedAndTrimmed()
    {
        var article = Article.Create("prykhovani-petli", Day, true, Texts("  Петлі  "), Now);

        Assert.Equal(["ru", "uk"], article.Translations.Select(t => t.Language));
        Assert.Equal("Петлі", article.Text("uk").Title);
        Assert.Null(article.CoverVersion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Upper")]
    [InlineData("double--hyphen")]
    [InlineData("-start")]
    [InlineData("пробіл")]
    public void Create_RejectsInvalidSlug(string slug)
        => Assert.Throws<DomainException>(() => Article.Create(slug, Day, true, Texts(), Now));

    [Fact]
    public void Update_RequiresEachLanguageOnce()
    {
        var article = Article.Create("a", Day, true, Texts(), Now);
        ArticleText[] onlyUk = [new("uk", "Т", "К", "<p>Т</p>"), new("uk", "Т2", "К", "<p>Т</p>")];

        Assert.Throws<DomainException>(() => article.Update("a", Day, true, onlyUk, Now));
    }

    [Fact]
    public void Cover_VersionGrowsAndResetsOnRemove()
    {
        var article = Article.Create("a", Day, true, Texts(), Now);

        article.ReplaceCover(Now);
        article.ReplaceCover(Now);
        Assert.Equal(2, article.CoverVersion);

        article.RemoveCover(Now);
        Assert.Null(article.CoverVersion);
    }

    [Theory]
    [InlineData("Як обрати дверну ручку?", "yak-obraty-dvernu-ruchku")]
    [InlineData("ZAMAK чи алюміній", "zamak-chy-aliuminii")]
    [InlineData("!!!", "article")]
    public void Slug_FromUkrainianTitle(string title, string expected)
        => Assert.Equal(expected, ArticleSlugs.FromTitle(title));

    [Fact]
    public void Slug_FromLongTitle_IsCutAtWordBoundary()
    {
        var slug = ArticleSlugs.FromTitle(string.Join(' ', Enumerable.Repeat("фурнітура", 20)));

        Assert.True(slug.Length <= 80);
        Assert.True(Article.IsValidSlug(slug));
        Assert.EndsWith("furnitura", slug);
    }

    [Fact]
    public void Input_RequiresBothLanguagesAndValidSlugWhenGiven()
    {
        var missingRu = new ArticleInput("Bad Slug", Day, true, [new ArticleTextInput("uk", "Т", "К", "<p>Т</p>")]);

        var result = new CreateArticleCommandValidator().Validate(new CreateArticleCommand(missingRu));

        Assert.Contains(result.Errors, e => e.PropertyName == "slug");
        Assert.Contains(result.Errors, e => e.PropertyName == "texts");
    }

    [Fact]
    public void Input_AllowsEmptySlugOnCreateOnly()
    {
        var input = new ArticleInput(null, Day, true,
            [new ArticleTextInput("uk", "Т", "К", "<p>Т</p>"), new ArticleTextInput("ru", "Т", "К", "<p>Т</p>")]);

        Assert.True(new CreateArticleCommandValidator().Validate(new CreateArticleCommand(input)).IsValid);
        Assert.False(new UpdateArticleCommandValidator().Validate(new UpdateArticleCommand(1, input)).IsValid);
    }
}
