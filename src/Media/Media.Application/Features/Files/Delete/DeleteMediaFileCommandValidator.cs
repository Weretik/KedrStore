namespace Media.Application.Features.Files.Delete;

public sealed class DeleteMediaFileCommandValidator : AbstractValidator<DeleteMediaFileCommand>
{
    public DeleteMediaFileCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
