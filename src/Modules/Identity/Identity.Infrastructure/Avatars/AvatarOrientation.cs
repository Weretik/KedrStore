using SkiaSharp;

namespace Identity.Infrastructure.Avatars;

/// <summary>Turns a square canvas so the photo shows the way the camera's EXIF origin says.</summary>
internal static class AvatarOrientation
{
    // Canvas transforms apply to points in reverse order of the calls.
    public static void Apply(SKCanvas canvas, SKEncodedOrigin origin, float centre)
    {
        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Scale(-1, 1, centre, centre);
                break;
            case SKEncodedOrigin.BottomRight:
                canvas.RotateDegrees(180, centre, centre);
                break;
            case SKEncodedOrigin.BottomLeft:
                canvas.Scale(1, -1, centre, centre);
                break;
            case SKEncodedOrigin.LeftTop:
                canvas.Scale(-1, 1, centre, centre);
                canvas.RotateDegrees(90, centre, centre);
                break;
            case SKEncodedOrigin.RightTop:
                canvas.RotateDegrees(90, centre, centre);
                break;
            case SKEncodedOrigin.RightBottom:
                canvas.Scale(1, -1, centre, centre);
                canvas.RotateDegrees(90, centre, centre);
                break;
            case SKEncodedOrigin.LeftBottom:
                canvas.RotateDegrees(270, centre, centre);
                break;
        }
    }
}
