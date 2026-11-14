namespace Media.Application.Features.Files.Commands.DeleteMediaFile;

public sealed class DeleteMediaFileValidator : AbstractValidator<DeleteMediaFileCommand>
{
    public DeleteMediaFileValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
