using BuildingBlocks.Application.Contracts;
using Work.Application.Contracts.Users;
using Work.Application.Features.Activity.Common;
using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Report.GetReport;

public sealed class GetWorkReportQueryHandler(IReadWorkDbContext db, ICurrentUserService user)
    : IQueryHandler<GetWorkReportQuery, Result<WorkReportDto>>
{
    public const int ActivityLimit = 2000;

    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

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

        return Result.Success(new WorkReportDto(
            owner,
            query.From,
            query.To,
            done.Select(WorkTaskDto.From).ToList(),
            open.Select(WorkTaskDto.From).ToList(),
            activity.Take(ActivityLimit).Select(WorkActivityDto.From).ToList(),
            activity.Count > ActivityLimit));
    }

    internal static DateTimeOffset KyivMidnightUtc(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, Kyiv.GetUtcOffset(local)).ToUniversalTime();
    }
}
