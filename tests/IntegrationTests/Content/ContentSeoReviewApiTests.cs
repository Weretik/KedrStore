using System.Net;
using System.Text.Json;
using Content.Domain.Articles;
using IntegrationTests.TestSupport.Api;
using static IntegrationTests.Content.Redirects.SiteRedirectsTestHost;

namespace IntegrationTests.Content;

/// <summary>Spec platform/010: the article part of the admin «SEO-огляд» over HTTP.</summary>
public sealed class ContentSeoReviewApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Review_ListsPublishedArticlesWithIssuesPerLanguage()
    {
        await using var db = CreateDb();
        db.Articles.Add(Article.Create("petli", new DateOnly(2026, 11, 4), true,
        [
            new ArticleText("uk", "Петлі", "Коротко.", "<p>Текст</p>", "Приховані петлі", "Як обрати приховані петлі."),
            new ArticleText("ru", "Петли", "Коротко.", "<p>Текст</p>"),
        ], Now, "seed").Value);
        db.Articles.Add(Article.Create("zamky", new DateOnly(2026, 11, 5), true,
        [
            new ArticleText("uk", "Приховані петлі", "Коротко.", "<p>Текст</p>", null, "Опис."),
            new ArticleText("ru", new string('я', 80), "Коротко.", "<p>Текст</p>", null, "Опис."),
        ], Now, "seed").Value);
        db.Articles.Add(Article.Create("chernetka", new DateOnly(2026, 11, 6), false,
            [new ArticleText("uk", "Чернетка", "Коротко.", "<p>Текст</p>"), new ArticleText("ru", "Черновик", "Коротко.", "<p>Текст</p>")],
            Now, "seed").Value);
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var seo = factory.CreateClientWithRole("Manager");

        using var json = JsonDocument.Parse(await seo.GetStringAsync("/api/admin/content/seo-review"));

        var items = json.RootElement.EnumerateArray()
            .Select(item => (Name: item.GetProperty("name").GetString(), Language: item.GetProperty("language").GetString(),
                Issues: string.Join(",", item.GetProperty("issues").EnumerateArray().Select(issue => issue.GetString()))))
            .ToList();
        Assert.Contains(("Петлі", "uk", "duplicateTitle"), items);
        Assert.Contains(("Приховані петлі", "uk", "duplicateTitle"), items);
        Assert.Contains(("Петли", "ru", "noDescription"), items);
        Assert.Contains((new string('я', 80), "ru", "longTitle"), items);
        Assert.DoesNotContain(items, item => item.Name is "Чернетка" or "Черновик");
        Assert.All(json.RootElement.EnumerateArray(), item => Assert.Equal("article", item.GetProperty("kind").GetString()));
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Review_IsForContentEditors(string? role, HttpStatusCode expected)
    {
        await using var db = CreateDb();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync("/api/admin/content/seo-review");

        Assert.Equal(expected, response.StatusCode);
    }
}
