namespace Media.Application.Features.Folders.Delete;

public sealed class DeleteMediaFolderCommandValidator : AbstractValidator<DeleteMediaFolderCommand>
{
    public DeleteMediaFolderCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
