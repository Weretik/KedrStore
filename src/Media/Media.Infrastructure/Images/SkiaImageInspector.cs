using Media.Application.Contracts.Images;
using SkiaSharp;

namespace Media.Infrastructure.Images;

/// <summary>Reads the picture format and size from its header with SkiaSharp, without decoding the pixels.</summary>
internal sealed class SkiaImageInspector : IImageInspector
{
    public ImageInfo? Inspect(Stream content)
    {
        using var codec = SKCodec.Create(new SKManagedStream(content, disposeManagedStream: false));
        if (codec is null)
            return null;

        var size = codec.Info;
        if (size.Width <= 0 || size.Height <= 0)
            return null;

        return codec.EncodedFormat switch
        {
            SKEncodedImageFormat.Jpeg => new ImageInfo("image/jpeg", ".jpg", size.Width, size.Height),
            SKEncodedImageFormat.Png => new ImageInfo("image/png", ".png", size.Width, size.Height),
            SKEncodedImageFormat.Webp => new ImageInfo("image/webp", ".webp", size.Width, size.Height),
            _ => null
        };
    }
}
