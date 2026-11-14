namespace Media.Application.Features.Files.Update;

public sealed class UpdateMediaFileCommandValidator : AbstractValidator<UpdateMediaFileCommand>
{
    public UpdateMediaFileCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FileName).Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Вкажіть назву файлу.")
            .Must(name => name is null || name.Trim().Length <= MediaFile.FileNameMaxLength)
            .WithMessage($"Назва файлу — не більше {MediaFile.FileNameMaxLength} символів.");
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
    }
}
