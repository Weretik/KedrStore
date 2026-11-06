namespace Work.Application.Features.Activity.Common;

public sealed record WorkActivityDto(
    long Id,
    Guid UserId,
    string UserEmail,
    string Area,
    string Operation,
    string Method,
    string Target,
    int Count,
    DateTimeOffset FirstAt,
    DateTimeOffset OccurredAt)
{
    public static WorkActivityDto From(WorkActivity activity) => new(
        activity.Id.Value,
        activity.UserId,
        activity.UserEmail,
        activity.Area,
        activity.Operation,
        activity.Method,
        activity.Target,
        activity.Count,
        activity.FirstAt,
        activity.OccurredAt);
}
