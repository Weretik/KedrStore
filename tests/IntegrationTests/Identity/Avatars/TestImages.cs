using SkiaSharp;

namespace IntegrationTests.Identity.Avatars;

internal static class TestImages
{
    /// <summary>A left half red, right half blue image, so a centre crop is visible in the result.</summary>
    public static byte[] Create(int width, int height, SKEncodedImageFormat format = SKEncodedImageFormat.Jpeg)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        surface.Canvas.Clear(SKColors.Red);
        using var paint = new SKPaint { Color = SKColors.Blue };
        surface.Canvas.DrawRect(width / 2f, 0, width / 2f, height, paint);
        using var image = surface.Snapshot();
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }

    /// <summary>Adds an EXIF orientation tag to a JPEG, as phone cameras do instead of rotating pixels.</summary>
    public static byte[] WithExifOrientation(byte[] jpeg, ushort orientation)
    {
        byte[] exif =
        [
            (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0,
            (byte)'M', (byte)'M', 0, 42, 0, 0, 0, 8, // big-endian TIFF header, first IFD at 8
            0, 1, // one entry
            0x01, 0x12, 0, 3, 0, 0, 0, 1, (byte)(orientation >> 8), (byte)orientation, 0, 0, // Orientation, SHORT
            0, 0, 0, 0 // no next IFD
        ];
        var length = exif.Length + 2;
        return [.. jpeg[..2], 0xFF, 0xE1, (byte)(length >> 8), (byte)length, .. exif, .. jpeg[2..]];
    }
}
