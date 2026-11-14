using BuildingBlocks.Application.Users;
using Work.Application.Abstractions.Users;
using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Report.Queries.GetWorkReport;

public sealed class GetWorkReportHandler(IReadWorkDbContext db, ICurrentUserService user, TimeProvider clock)
    : IQueryHandler<GetWorkReportQuery, Result<WorkReportDto>>
{
    public const int ActivityLimit = 2000;

    public async ValueTask<Result<WorkReportDto>> Handle(GetWorkReportQuery query, CancellationToken cancellationToken)
    {
        var userId = query.UserId ?? WorkViewer.UserId(user);
        if (userId is not { } owner)
            return Result<WorkReportDto>.Unauthorized();

        var fromUtc = KyivMidnightUtc(query.From);
        var toUtc = KyivMidnightUtc(query.To.AddDays(1));

        var done = await db.WorkTasks.AsNoTracking()
            .Where(task => task.OwnerUserId == owner && task.Status == WorkTaskStatus.Done
                           && task.CompletedAt >= fromUtc && task.CompletedAt < toUtc)
            .OrderByDescending(task => task.CompletedAt)
            .ToListAsync(cancellationToken);
        var open = await db.WorkTasks.AsNoTracking()
            .Where(task => task.OwnerUserId == owner && task.Status != WorkTaskStatus.Done)
            .OrderByDescending(task => task.UpdatedAt)
            .ToListAsync(cancellationToken);
        var activity = await db.WorkActivities.AsNoTracking()
            .Where(entry => entry.UserId == owner && entry.OccurredAt >= fromUtc && entry.OccurredAt < toUtc)
            .OrderByDescending(entry => entry.OccurredAt)
            .Take(ActivityLimit + 1)
            .ToListAsync(cancellationToken);

        var today = WorkCalendar.Today(clock);
        return Result.Success(new WorkReportDto(
            owner,
            query.From,
            query.To,
            done.Select(task => WorkTaskDto.From(task, today)).ToList(),
            open.Select(task => WorkTaskDto.From(task, today)).ToList(),
            activity.Take(ActivityLimit).Select(WorkActivityDto.From).ToList(),
            activity.Count > ActivityLimit));
    }

    internal static DateTimeOffset KyivMidnightUtc(DateOnly date) => WorkCalendar.MidnightUtc(date);
}
