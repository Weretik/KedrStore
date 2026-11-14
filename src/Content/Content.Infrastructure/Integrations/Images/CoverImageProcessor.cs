using Content.Application.Abstractions.Media;
using SkiaSharp;

namespace Content.Infrastructure.Integrations.Images;

/// <summary>
/// Checks an uploaded cover and stores it as WebP at most <see cref="MaxWidth"/> pixels wide,
/// with the EXIF orientation applied and metadata dropped.
/// </summary>
internal sealed class CoverImageProcessor : ICoverImageProcessor
{
    public const int MaxWidth = 1600;
    public const int MinWidth = 600;
    public const long MaxUploadBytes = 10 * 1024 * 1024;
    public const long MaxPixels = 40_000_000;
    public const string FileField = "file";

    private const int WebpQuality = 80;

    private static readonly SKEncodedImageFormat[] SupportedFormats =
        [SKEncodedImageFormat.Jpeg, SKEncodedImageFormat.Png, SKEncodedImageFormat.Webp];

    public async Task<Result<ProcessedCover>> ProcessAsync(CoverUpload upload, CancellationToken cancellationToken = default)
    {
        if (upload.Length > MaxUploadBytes)
            return Invalid("The picture is larger than 10 MB.");

        var bytes = await ReadLimitedAsync(upload.OpenReadStream, cancellationToken);
        if (bytes is null)
            return Invalid("The picture is larger than 10 MB.");

        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data);
        if (codec is null || !SupportedFormats.Contains(codec.EncodedFormat))
            return Invalid("Only JPEG, PNG and WebP pictures are supported.");

        var rotated = IsQuarterTurn(codec.EncodedOrigin);
        var (width, height) = rotated
            ? (codec.Info.Height, codec.Info.Width)
            : (codec.Info.Width, codec.Info.Height);
        if (width < MinWidth)
            return Invalid($"The picture must be at least {MinWidth} pixels wide.");
        if ((long)width * height > MaxPixels)
            return Invalid("The picture resolution is too high (more than 40 megapixels).");

        using var decoded = SKBitmap.Decode(codec);
        if (decoded is null)
            return Invalid("Only JPEG, PNG and WebP pictures are supported.");

        var scale = Math.Min(1f, (float)MaxWidth / width);
        var outWidth = Math.Max(1, (int)Math.Round(width * scale));
        var outHeight = Math.Max(1, (int)Math.Round(height * scale));

        using var surface = SKSurface.Create(new SKImageInfo(outWidth, outHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.Scale(scale);
        Orient(canvas, codec.EncodedOrigin, codec.Info.Width, codec.Info.Height);
        using (var image = SKImage.FromBitmap(decoded))
        using (var paint = new SKPaint { IsAntialias = true })
        {
            canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint);
        }

        canvas.Flush();
        using var snapshot = surface.Snapshot();
        using var encoded = snapshot.Encode(SKEncodedImageFormat.Webp, WebpQuality);
        var webp = encoded.ToArray();
        return webp.Length > ArticleCover.MaxBytes
            ? Invalid("The picture is too detailed to store; try a smaller one.")
            : Result.Success(new ProcessedCover(webp, "image/webp"));
    }

    private static bool IsQuarterTurn(SKEncodedOrigin origin) => origin is SKEncodedOrigin.LeftTop
        or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

    // Canvas transforms apply to points in reverse order of the calls; w and h are the stored pixel sizes.
    private static void Orient(SKCanvas canvas, SKEncodedOrigin origin, int w, int h)
    {
        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Scale(-1, 1, w / 2f, 0);
                break;
            case SKEncodedOrigin.BottomRight:
                canvas.RotateDegrees(180, w / 2f, h / 2f);
                break;
            case SKEncodedOrigin.BottomLeft:
                canvas.Scale(1, -1, 0, h / 2f);
                break;
            case SKEncodedOrigin.LeftTop:
                canvas.Scale(-1, 1, h / 2f, 0);
                canvas.Translate(h, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightTop:
                canvas.Translate(h, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom:
                canvas.Scale(-1, 1, h / 2f, 0);
                canvas.Translate(0, w);
                canvas.RotateDegrees(270);
                break;
            case SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, w);
                canvas.RotateDegrees(270);
                break;
        }
    }

    private static async Task<byte[]?> ReadLimitedAsync(Func<Stream> open, CancellationToken cancellationToken)
    {
        await using var content = open();
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaxUploadBytes)
                return null;
            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    // The identifier names the multipart field, so the admin shows the message next to the file picker.
    private static Result<ProcessedCover> Invalid(string message) =>
        Result<ProcessedCover>.Invalid(new ValidationError(FileField, message, null, ValidationSeverity.Error));
}
