namespace Work.Application.Features.Activity.Commands.RecordWorkActivity;

public sealed class RecordWorkActivityValidator : AbstractValidator<RecordWorkActivityCommand>
{
    public RecordWorkActivityValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Area).NotEmpty();
        RuleFor(x => x.Operation).NotEmpty();
        RuleFor(x => x.Method).NotEmpty();
    }
}
