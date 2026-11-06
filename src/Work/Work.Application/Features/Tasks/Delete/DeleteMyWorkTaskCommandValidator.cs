namespace Work.Application.Features.Tasks.Delete;

public sealed class DeleteMyWorkTaskCommandValidator : AbstractValidator<DeleteMyWorkTaskCommand>
{
    public DeleteMyWorkTaskCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
