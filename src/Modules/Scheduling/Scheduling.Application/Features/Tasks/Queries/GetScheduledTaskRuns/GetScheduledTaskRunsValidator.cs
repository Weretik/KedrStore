namespace Scheduling.Application.Features.Tasks.Queries.GetScheduledTaskRuns;

public sealed class GetScheduledTaskRunsValidator : AbstractValidator<GetScheduledTaskRunsQuery>
{
    public GetScheduledTaskRunsValidator() => RuleFor(x => x.Key).NotEmpty().MaximumLength(64);
}
