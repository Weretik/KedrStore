namespace Media.Application.Features.Folders.Common;

/// <summary>Shared rules for a folder name and its parent.</summary>
internal static class MediaFolderInputValidator
{
    public static void NameRules<T>(IRuleBuilder<T, string> rule)
        => rule.Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Вкажіть назву.")
            .Must(name => name is null || name.Trim().Length <= MediaFolder.NameMaxLength)
            .WithMessage($"Назва — не більше {MediaFolder.NameMaxLength} символів.");
}
