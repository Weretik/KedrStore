using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.ChangeStatus;

public sealed class ChangeMyWorkTaskStatusCommandValidator : AbstractValidator<ChangeMyWorkTaskStatusCommand>
{
    public ChangeMyWorkTaskStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Status).Must(status => WorkTaskStatusNames.TryParse(status, out _))
            .WithMessage("Status must be Planned, InProgress or Done.");
        RuleFor(x => x.Result).MaximumLength(WorkTask.ResultMaxLength);
    }
}
