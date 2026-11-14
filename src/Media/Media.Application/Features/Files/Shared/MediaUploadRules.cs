using Media.Application.Abstractions.Images;

namespace Media.Application.Features.Files.Shared;

/// <summary>Validation shared by upload and replace.</summary>
public static class MediaUploadRules
{
    /// <summary>The largest picture the library accepts (R-4).</summary>
    public const long MaxUploadBytes = 15 * 1024 * 1024;

    internal static IRuleBuilderOptions<T, MediaUpload> FileRules<T>(IRuleBuilder<T, MediaUpload> rule)
        => rule.NotNull().WithMessage("Додайте файл.")
            .Must(file => file is null || file.Length is > 0 and <= MaxUploadBytes)
            .WithMessage("Зображення має бути не більше 15 МБ.");
}
