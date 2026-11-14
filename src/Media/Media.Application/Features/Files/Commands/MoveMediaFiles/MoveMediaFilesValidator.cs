namespace Media.Application.Features.Files.Commands.MoveMediaFiles;

public sealed class MoveMediaFilesValidator : AbstractValidator<MoveMediaFilesCommand>
{
    public const int MaxFiles = 500;

    public MoveMediaFilesValidator()
    {
        RuleFor(x => x.FileIds).NotEmpty().Must(ids => ids is null || ids.Count <= MaxFiles)
            .WithMessage($"За раз можна перемістити не більше {MaxFiles} файлів.");
        RuleForEach(x => x.FileIds).GreaterThan(0);
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
    }
}
