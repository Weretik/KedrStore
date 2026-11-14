namespace Media.Application.Features.Folders.Commands.DeleteMediaFolder;

public sealed class DeleteMediaFolderValidator : AbstractValidator<DeleteMediaFolderCommand>
{
    public DeleteMediaFolderValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
