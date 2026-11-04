namespace Media.Application.Features.Files.Move;

public sealed class MoveMediaFilesCommandValidator : AbstractValidator<MoveMediaFilesCommand>
{
    public const int MaxFiles = 500;

    public MoveMediaFilesCommandValidator()
    {
        RuleFor(x => x.FileIds).NotEmpty().Must(ids => ids is null || ids.Count <= MaxFiles)
            .WithMessage($"Move at most {MaxFiles} files at once.");
        RuleForEach(x => x.FileIds).GreaterThan(0);
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
    }
}
