using Content.Application.Abstractions.Media;
using Content.Domain.Articles;
using Content.Infrastructure.Integrations.Html;
using Content.Infrastructure.Integrations.Images;
using Content.Infrastructure.Persistence.Seed;
using IntegrationTests.Identity.Avatars;
using SkiaSharp;

namespace IntegrationTests.Content;

/// <summary>Infrastructure pieces of the Content module that need no database.</summary>
public sealed class ContentInfrastructureTests
{
    private readonly CoverImageProcessor _processor = new();

    [Fact]
    public async Task Cover_WideImage_IsResizedToMaxWidthWebp()
    {
        var result = await ProcessAsync(TestImages.Create(3200, 1600));

        Assert.True(result.IsSuccess, string.Join(", ", result.ValidationErrors.Select(e => e.ErrorMessage)));
        Assert.Equal("image/webp", result.Value.ContentType);
        using var codec = SKCodec.Create(new MemoryStream(result.Value.Data));
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
        Assert.Equal(CoverImageProcessor.MaxWidth, codec.Info.Width);
        Assert.Equal(800, codec.Info.Height);
    }

    [Fact]
    public async Task Cover_ExifOrientation_IsApplied()
    {
        // Orientation 6 = rotate 90° clockwise: a 1200x800 stored photo shows 800x1200 with red (left) on top.
        var result = await ProcessAsync(TestImages.WithExifOrientation(TestImages.Create(1200, 800), 6));

        using var bitmap = SKBitmap.Decode(result.Value.Data);
        Assert.Equal(800, bitmap.Width);
        Assert.Equal(1200, bitmap.Height);
        var top = bitmap.GetPixel(400, 10);
        var bottom = bitmap.GetPixel(400, 1190);
        Assert.True(top.Red > 200 && top.Blue < 60, $"top pixel {top}");
        Assert.True(bottom.Blue > 200 && bottom.Red < 60, $"bottom pixel {bottom}");
    }

    [Fact]
    public async Task Cover_NarrowOrUnknownImage_IsRejectedOnFileField()
    {
        var narrow = await ProcessAsync(TestImages.Create(300, 300));
        var text = await ProcessAsync("not an image"u8.ToArray());

        Assert.Equal(CoverImageProcessor.FileField, Assert.Single(narrow.ValidationErrors).Identifier);
        Assert.Equal(CoverImageProcessor.FileField, Assert.Single(text.ValidationErrors).Identifier);
    }

    [Fact]
    public void Sanitizer_KeepsFormattingAndDropsScripts()
    {
        var html = new ArticleHtmlSanitizer().Sanitize(
            "<h2 style=\"color:red\">Заголовок</h2><p onclick=\"x()\">Текст <a href=\"javascript:alert(1)\">x</a>" +
            "<a href=\"/uk/catalog\">каталог</a></p><script>alert(1)</script><img src=x>");

        Assert.Equal("<h2>Заголовок</h2><p>Текст <a>x</a><a href=\"/uk/catalog\">каталог</a></p>", html);
    }

    [Fact]
    public void LegacyArticles_AreValidDomainArticlesWithEmbeddedCovers()
    {
        Assert.Equal([1L, 2, 3, 4, 5, 6], LegacyArticles.All.Select(a => a.Id));
        foreach (var legacy in LegacyArticles.All)
        {
            var article = Article.Create(legacy.Slug, legacy.PublishedOn, true,
                legacy.Texts.Select(t => new ArticleText(t.Language, t.Title, t.Excerpt, t.BodyHtml)).ToList(),
                DateTimeOffset.UnixEpoch, "seed");
            Assert.Equal(2, article.Translations.Count);

            var name = $"Content.Infrastructure.Seed.Covers.article-{legacy.Id}.webp";
            using var stream = typeof(LegacyArticles).Assembly.GetManifestResourceStream(name);
            Assert.NotNull(stream);
            Assert.InRange(stream.Length, 1, ArticleCover.MaxBytes);
        }
    }

    private Task<Ardalis.Result.Result<ProcessedCover>> ProcessAsync(byte[] bytes)
        => _processor.ProcessAsync(new CoverUpload("cover.jpg", "image/jpeg", bytes.Length, () => new MemoryStream(bytes)));
}
