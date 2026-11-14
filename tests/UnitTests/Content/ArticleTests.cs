using UnitTests.BuildingBlocks.Results;
using Content.Application.Features.Articles.Commands.CreateArticle;
using Content.Application.Features.Articles.Commands.UpdateArticle;
using Content.Application.Features.Articles.Shared;
using Content.Domain.Articles;
using Content.Application.Features.Articles.Shared.Reading;
using Content.Application.Features.Articles.Shared.Writing;

namespace UnitTests.Content;

public sealed class ArticleTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Day = new(2026, 11, 4);
    private const string Editor = "manager@classkedr.com.ua";

    private static ArticleText[] Texts(string ukTitle = "Петлі") =>
    [
        new("ru", "Петли", "Коротко.", "<p>Текст</p>"),
        new("uk", ukTitle, "Коротко.", "<p>Текст</p>"),
    ];

    [Fact]
    public void Create_KeepsBothLanguagesSortedAndTrimmed()
    {
        var article = Article.Create("prykhovani-petli", Day, true, Texts("  Петлі  "), Now, " manager@classkedr.com.ua ").Value;

        Assert.Equal(["ru", "uk"], article.Translations.Select(t => t.Language));
        Assert.Equal("Петлі", article.Text("uk").Title);
        Assert.Null(article.CoverVersion);
        Assert.Equal("manager@classkedr.com.ua", article.UpdatedBy);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Upper")]
    [InlineData("double--hyphen")]
    [InlineData("-start")]
    [InlineData("пробіл")]
    public void Create_RejectsInvalidSlug(string slug)
        => DomainAssert.Fails(Article.Create(slug, Day, true, Texts(), Now, Editor));

    [Fact]
    public void Update_RequiresEachLanguageOnce()
    {
        var article = Article.Create("a", Day, true, Texts(), Now, Editor).Value;
        ArticleText[] onlyUk = [new("uk", "Т", "К", "<p>Т</p>"), new("uk", "Т2", "К", "<p>Т</p>")];

        DomainAssert.Fails(article.Update("a", Day, true, onlyUk, Now, Editor));
    }

    [Fact]
    public void Cover_IsALibraryFileAndRemovable()
    {
        var article = Article.Create("a", Day, true, Texts(), Now, Editor).Value;
        Assert.Null(article.CoverMediaFileId);

        article.SetCover(42, Now, Editor);
        Assert.Equal(42, article.CoverMediaFileId);
        Assert.Null(article.CoverVersion);
        DomainAssert.Fails(article.SetCover(0, Now, Editor));

        article.RemoveCover(Now, "");
        Assert.Null(article.CoverMediaFileId);
        Assert.Equal("unknown", article.UpdatedBy);
    }

    [Fact]
    public void Cover_SameFileAgainChangesNothing()
    {
        var article = Article.Create("a", Day, true, Texts(), Now, Editor).Value;
        article.SetCover(42, Now, Editor);

        article.SetCover(42, Now.AddDays(1), "other@classkedr.com.ua");

        Assert.Equal(Now, article.UpdatedAt);
        Assert.Equal(Editor, article.UpdatedBy);
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

        var result = new CreateArticleValidator().Validate(new CreateArticleCommand(missingRu));

        Assert.Contains(result.Errors, e => e.PropertyName == "slug");
        Assert.Contains(result.Errors, e => e.PropertyName == "texts");
    }

    [Fact]
    public void Input_RejectsCoverTogetherWithRemove()
    {
        var input = new ArticleInput("a", Day, true,
            [new ArticleTextInput("uk", "Т", "К", "<p>Т</p>"), new ArticleTextInput("ru", "Т", "К", "<p>Т</p>")],
            CoverMediaFileId: 5, RemoveCover: true);

        var result = new UpdateArticleValidator().Validate(new UpdateArticleCommand(1, input));

        Assert.Contains(result.Errors, e => e.PropertyName == "removeCover");
    }

    [Fact]
    public void Input_AllowsEmptySlugOnCreateOnly()
    {
        var input = new ArticleInput(null, Day, true,
            [new ArticleTextInput("uk", "Т", "К", "<p>Т</p>"), new ArticleTextInput("ru", "Т", "К", "<p>Т</p>")]);

        Assert.True(new CreateArticleValidator().Validate(new CreateArticleCommand(input)).IsValid);
        Assert.False(new UpdateArticleValidator().Validate(new UpdateArticleCommand(1, input)).IsValid);
    }
}
