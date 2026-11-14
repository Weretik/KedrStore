namespace Work.Application.Features.Report.GetReport;

public sealed class GetWorkReportQueryValidator : AbstractValidator<GetWorkReportQuery>
{
    public const int MaxDays = 92;

    public GetWorkReportQueryValidator()
    {
        RuleFor(x => x.UserId).NotEqual(Guid.Empty).When(x => x.UserId is not null);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("Дата «по» не може бути раніше за дату «з».");
        RuleFor(x => x).Must(x => x.To.DayNumber - x.From.DayNumber < MaxDays)
            .WithName("to").WithMessage($"Період — не більше {MaxDays} днів.");
    }
}
