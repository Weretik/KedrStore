namespace Media.Application.Features.Files.Update;

public sealed class UpdateMediaFileCommandValidator : AbstractValidator<UpdateMediaFileCommand>
{
    public UpdateMediaFileCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FileName).Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("File name is required.")
            .Must(name => name is null || name.Trim().Length <= MediaFile.FileNameMaxLength)
            .WithMessage($"File name must be at most {MediaFile.FileNameMaxLength} characters.");
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
    }
}
