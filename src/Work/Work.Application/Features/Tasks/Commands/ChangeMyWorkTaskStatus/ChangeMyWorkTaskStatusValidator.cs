using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.ChangeMyWorkTaskStatus;

public sealed class ChangeMyWorkTaskStatusValidator : AbstractValidator<ChangeMyWorkTaskStatusCommand>
{
    public ChangeMyWorkTaskStatusValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Status).Must(status => WorkTaskStatusNames.TryParse(status, out _))
            .WithMessage("Статус: Planned, InProgress або Done.");
        RuleFor(x => x.Result).MaximumLength(WorkTask.ResultMaxLength);
    }
}
