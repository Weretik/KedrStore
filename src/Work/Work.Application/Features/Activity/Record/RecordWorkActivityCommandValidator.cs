namespace Work.Application.Features.Activity.Record;

public sealed class RecordWorkActivityCommandValidator : AbstractValidator<RecordWorkActivityCommand>
{
    public RecordWorkActivityCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Area).NotEmpty();
        RuleFor(x => x.Operation).NotEmpty();
        RuleFor(x => x.Method).NotEmpty();
    }
}
