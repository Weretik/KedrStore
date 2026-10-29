using Ardalis.Result;
using Identity.Application.Features.Avatars;
using SkiaSharp;

namespace Identity.Infrastructure.Avatars;

/// <summary>
/// Checks an uploaded image against <see cref="AvatarRules"/> and turns it into a square WebP:
/// centre crop, EXIF orientation applied, metadata dropped.
/// </summary>
public sealed class AvatarImageProcessor
{
    public const string TooLargeMessage = "The photo is larger than 2 MB.";
    public const string UnsupportedFormatMessage = "Only JPEG, PNG and WebP photos are supported.";
    public const string TooSmallMessage = "The photo must be at least 128×128 pixels.";
    public const string TooManyPixelsMessage = "The photo resolution is too high (more than 24 megapixels).";

    private const int WebpQuality = 85;

    private static readonly SKEncodedImageFormat[] SupportedFormats =
        [SKEncodedImageFormat.Jpeg, SKEncodedImageFormat.Png, SKEncodedImageFormat.Webp];

    public async Task<Result<byte[]>> ProcessAsync(AvatarUpload upload, CancellationToken cancellationToken = default)
    {
        if (upload.Length > AvatarRules.MaxBytes)
        {
            return Invalid(TooLargeMessage);
        }

        var bytes = await ReadLimitedAsync(upload.Content, cancellationToken);
        if (bytes is null)
        {
            return Invalid(TooLargeMessage);
        }

        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data);
        if (codec is null || !SupportedFormats.Contains(codec.EncodedFormat))
        {
            return Invalid(UnsupportedFormatMessage);
        }

        var (width, height) = (codec.Info.Width, codec.Info.Height);
        if (Math.Min(width, height) < AvatarRules.MinSide)
        {
            return Invalid(TooSmallMessage);
        }

        if ((long)width * height > AvatarRules.MaxPixels)
        {
            return Invalid(TooManyPixelsMessage);
        }

        using var decoded = Decode(codec);
        if (decoded is null)
        {
            return Invalid(UnsupportedFormatMessage);
        }

        using var avatar = RenderSquare(decoded, codec.EncodedOrigin);
        using var encoded = avatar.Encode(SKEncodedImageFormat.Webp, WebpQuality);
        return Result.Success(encoded.ToArray());
    }

    private static async Task<byte[]?> ReadLimitedAsync(Stream content, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > AvatarRules.MaxBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    /// <summary>Decodes at the smallest codec scale that still covers the output (JPEG and WebP decode scaled).</summary>
    private static SKBitmap? Decode(SKCodec codec)
    {
        var shortSide = Math.Min(codec.Info.Width, codec.Info.Height);
        var scale = Math.Max(1f * AvatarRules.OutputSide / shortSide, 1f / 8);
        var size = codec.GetScaledDimensions(Math.Min(scale, 1f));
        if (Math.Min(size.Width, size.Height) < AvatarRules.OutputSide)
        {
            size = codec.Info.Size;
        }

        var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        var bitmap = new SKBitmap(info);
        var result = codec.GetPixels(info, bitmap.GetPixels());
        if (result is SKCodecResult.Success or SKCodecResult.IncompleteInput)
        {
            return bitmap;
        }

        bitmap.Dispose();
        return null;
    }

    private static SKImage RenderSquare(SKBitmap source, SKEncodedOrigin origin)
    {
        // The centre square of the stored pixels is the centre square of the displayed photo,
        // so cropping first and orienting the square afterwards gives the same result.
        var side = Math.Min(source.Width, source.Height);
        var crop = SKRectI.Create((source.Width - side) / 2, (source.Height - side) / 2, side, side);

        using var surface = SKSurface.Create(new SKImageInfo(AvatarRules.OutputSide, AvatarRules.OutputSide, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        AvatarOrientation.Apply(canvas, origin, AvatarRules.OutputSide / 2f);

        using var image = SKImage.FromBitmap(source);
        using var paint = new SKPaint { IsAntialias = true };
        canvas.DrawImage(
            image,
            crop,
            SKRect.Create(AvatarRules.OutputSide, AvatarRules.OutputSide),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),
            paint);
        canvas.Flush();
        return surface.Snapshot();
    }

    private static Result<byte[]> Invalid(string message) =>
        Result<byte[]>.Invalid(new ValidationError(message));
}
