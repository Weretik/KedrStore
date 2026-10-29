using Identity.Application.Features.Avatars;
using Identity.Infrastructure.Avatars;
using SkiaSharp;

namespace IntegrationTests.Identity.Avatars;

public sealed class AvatarImageProcessorTests
{
    private readonly AvatarImageProcessor _processor = new();

    [Theory]
    [InlineData(SKEncodedImageFormat.Jpeg, 1000, 600)]
    [InlineData(SKEncodedImageFormat.Png, 300, 900)]
    [InlineData(SKEncodedImageFormat.Webp, 128, 128)]
    public async Task Supported_Image_Becomes_Square_Webp(SKEncodedImageFormat format, int width, int height)
    {
        var result = await ProcessAsync(TestImages.Create(width, height, format));

        Assert.True(result.IsSuccess, string.Join(", ", result.ValidationErrors.Select(e => e.ErrorMessage)));
        using var codec = SKCodec.Create(new MemoryStream(result.Value));
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
        Assert.Equal(AvatarRules.OutputSide, codec.Info.Width);
        Assert.Equal(AvatarRules.OutputSide, codec.Info.Height);
    }

    [Fact]
    public async Task Wide_Image_Is_Cropped_From_The_Centre()
    {
        // Red left half, blue right half: the centre square keeps both colours side by side.
        var result = await ProcessAsync(TestImages.Create(1000, 400));

        using var bitmap = SKBitmap.Decode(result.Value);
        var left = bitmap.GetPixel(10, AvatarRules.OutputSide / 2);
        var right = bitmap.GetPixel(AvatarRules.OutputSide - 10, AvatarRules.OutputSide / 2);
        Assert.True(left.Red > 200 && left.Blue < 60, $"left pixel {left}");
        Assert.True(right.Blue > 200 && right.Red < 60, $"right pixel {right}");
    }

    [Fact]
    public async Task Exif_Orientation_Is_Applied()
    {
        // Orientation 6 means "rotate 90° clockwise to display": the red left half ends up on top.
        var result = await ProcessAsync(TestImages.WithExifOrientation(TestImages.Create(400, 200), 6));

        using var bitmap = SKBitmap.Decode(result.Value);
        var top = bitmap.GetPixel(AvatarRules.OutputSide / 2, 10);
        var bottom = bitmap.GetPixel(AvatarRules.OutputSide / 2, AvatarRules.OutputSide - 10);
        Assert.True(top.Red > 200 && top.Blue < 60, $"top pixel {top}");
        Assert.True(bottom.Blue > 200 && bottom.Red < 60, $"bottom pixel {bottom}");
    }

    [Fact]
    public async Task File_Over_2_MB_Is_Rejected()
    {
        var result = await _processor.ProcessAsync(new AvatarUpload(new MemoryStream(new byte[10]), AvatarRules.MaxBytes + 1));

        Assert.Equal(AvatarImageProcessor.TooLargeMessage, Assert.Single(result.ValidationErrors).ErrorMessage);
    }

    [Fact]
    public async Task Stream_Longer_Than_Declared_Is_Rejected()
    {
        var bytes = new byte[AvatarRules.MaxBytes + 10];
        var result = await _processor.ProcessAsync(new AvatarUpload(new MemoryStream(bytes), 100));

        Assert.Equal(AvatarImageProcessor.TooLargeMessage, Assert.Single(result.ValidationErrors).ErrorMessage);
    }

    [Fact]
    public async Task Small_Image_Is_Rejected()
    {
        var result = await ProcessAsync(TestImages.Create(100, 100));

        Assert.Equal(AvatarImageProcessor.TooSmallMessage, Assert.Single(result.ValidationErrors).ErrorMessage);
    }

    [Fact]
    public async Task Text_File_Is_Rejected()
    {
        var result = await ProcessAsync("not an image"u8.ToArray());

        Assert.Equal(AvatarImageProcessor.UnsupportedFormatMessage, Assert.Single(result.ValidationErrors).ErrorMessage);
    }

    [Fact]
    public async Task Gif_Is_Rejected()
    {
        // GIF89a header with a 1x1 image.
        var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==");
        var result = await ProcessAsync(gif);

        Assert.Equal(AvatarImageProcessor.UnsupportedFormatMessage, Assert.Single(result.ValidationErrors).ErrorMessage);
    }

    private Task<Ardalis.Result.Result<byte[]>> ProcessAsync(byte[] bytes) =>
        _processor.ProcessAsync(new AvatarUpload(new MemoryStream(bytes), bytes.Length));
}
