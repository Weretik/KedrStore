namespace Work.Application.Features.Tasks.Commands.DeleteMyWorkTask;

public sealed class DeleteMyWorkTaskValidator : AbstractValidator<DeleteMyWorkTaskCommand>
{
    public DeleteMyWorkTaskValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
