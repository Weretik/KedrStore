namespace Identity.Application.Features.Avatars;

/// <summary>Limits for an uploaded avatar and the stored result.</summary>
public static class AvatarRules
{
    public const long MaxBytes = 2 * 1024 * 1024;
    public const int MinSide = 128;

    // Guards against decompression bombs: a small file that decodes to a huge bitmap.
    public const long MaxPixels = 24_000_000;
    public const int OutputSide = 256;
}
