using UnitTests.BuildingBlocks.Results;
using Content.Domain.Gallery;
namespace UnitTests.Content.Documents;

public sealed class GalleryItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=0FDRbSaQ9Ec")]
    [InlineData("https://www.youtube.com/watch?feature=share&v=0FDRbSaQ9Ec&t=42s")]
    [InlineData("https://m.youtube.com/watch?v=0FDRbSaQ9Ec")]
    [InlineData("https://youtu.be/0FDRbSaQ9Ec?si=abc")]
    [InlineData("youtu.be/0FDRbSaQ9Ec")]
    [InlineData("https://www.youtube.com/shorts/0FDRbSaQ9Ec")]
    [InlineData("https://www.youtube.com/embed/0FDRbSaQ9Ec")]
    [InlineData("https://www.youtube-nocookie.com/embed/0FDRbSaQ9Ec")]
    [InlineData("https://www.youtube.com/live/0FDRbSaQ9Ec?feature=shared")]
    [InlineData("  0FDRbSaQ9Ec ")]
    public void YoutubeVideoId_ReadsTheIdFromAnyCommonLink(string value)
    {
        Assert.True(YoutubeVideoId.TryParse(value, out var id));
        Assert.Equal("0FDRbSaQ9Ec", id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://vimeo.com/0FDRbSaQ9Ec")]
    [InlineData("https://www.youtube.com/@kedrclass2334/videos")]
    [InlineData("https://www.youtube.com/watch?v=short")]
    [InlineData("https://evil.example/watch?v=0FDRbSaQ9Ec")]
    [InlineData("javascript:alert(1)")]
    public void YoutubeVideoId_RejectsEverythingElse(string? value)
    {
        Assert.False(YoutubeVideoId.TryParse(value, out _));
        DomainAssert.Fails(YoutubeVideoId.Parse(value));
    }

    [Fact]
    public void CreateVideo_StoresTheIdOnly_AndTrimsTexts()
    {
        var item = GalleryItem.CreateVideo("https://youtu.be/0FDRbSaQ9Ec",
            3, GalleryItemTexts.Create(" Петля ", "  ", null, "Подпись").Value, Now).Value;

        Assert.Equal(GalleryItemKind.Video, item.Kind);
        Assert.Equal("0FDRbSaQ9Ec", item.YoutubeId);
        Assert.Null(item.MediaFileId);
        Assert.Equal(3, item.Position);
        Assert.Equal("Петля", item.TitleUk);
        Assert.Null(item.TitleRu);
        Assert.Equal("Подпись", item.CaptionRu);
    }

    [Fact]
    public void CreatePhoto_NeedsAPositiveFileId_AndTextsHaveLimits()
    {
        var photo = GalleryItem.CreatePhoto(42, 0, GalleryItemTexts.Empty, Now).Value;

        Assert.Equal(GalleryItemKind.Photo, photo.Kind);
        Assert.Equal(42, photo.MediaFileId);
        Assert.Null(photo.YoutubeId);
        DomainAssert.Fails(GalleryItem.CreatePhoto(0, 0, GalleryItemTexts.Empty, Now));
        DomainAssert.Fails(GalleryItemTexts.Create(new string('а', GalleryItemTexts.TitleMaxLength + 1), null, null, null));
        DomainAssert.Fails(photo.MoveTo(-1, Now));
    }
}
