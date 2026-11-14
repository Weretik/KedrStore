using FluentValidation;

namespace Identity.Application.Features.Avatars;

/// <summary>Limits for an uploaded avatar and the stored result, with the messages shown under the file field.</summary>
public static class AvatarRules
{
    public const long MaxBytes = 2 * 1024 * 1024;
    public const int MinSide = 128;

    // Guards against decompression bombs: a small file that decodes to a huge bitmap.
    public const long MaxPixels = 24_000_000;
    public const int OutputSide = 256;

    public const string FileField = "file";
    public const string MissingMessage = "Виберіть фото.";
    public const string TooLargeMessage = "Фото більше за 2 МБ.";
    public const string UnsupportedFormatMessage = "Підтримуються лише фото JPEG, PNG і WebP.";
    public const string TooSmallMessage = "Фото має бути не менше 128×128 пікселів.";
    public const string TooManyPixelsMessage = "Роздільна здатність фото завелика (понад 24 мегапікселі).";

    /// <summary>The upload size rule; the image itself is checked when it is processed.</summary>
    public static IRuleBuilderOptions<T, long> AvatarSize<T>(this IRuleBuilder<T, long> rule)
        => rule.GreaterThan(0).WithMessage(MissingMessage)
            .LessThanOrEqualTo(MaxBytes).WithMessage(TooLargeMessage);
}
