using BuildingBlocks.Application.Contracts;
using Work.Application.Abstractions.Users;
using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Queries.GetMyWorkTasks;

public sealed class GetMyWorkTasksHandler(IReadWorkDbContext db, ICurrentUserService user, TimeProvider clock)
    : IQueryHandler<GetMyWorkTasksQuery, Result<IReadOnlyList<WorkTaskDto>>>
{
    public static readonly TimeSpan DoneVisibleFor = TimeSpan.FromDays(30);

    public async ValueTask<Result<IReadOnlyList<WorkTaskDto>>> Handle(GetMyWorkTasksQuery query,
        CancellationToken cancellationToken)
    {
        if (WorkViewer.UserId(user) is not { } viewer)
            return Result<IReadOnlyList<WorkTaskDto>>.Unauthorized();

        var doneSince = clock.GetUtcNow() - DoneVisibleFor;
        var tasks = await db.WorkTasks.AsNoTracking()
            .Where(task => task.OwnerUserId == viewer
                           && (task.Status != WorkTaskStatus.Done || task.CompletedAt >= doneSince))
            .OrderByDescending(task => task.UpdatedAt)
            .ToListAsync(cancellationToken);

        var today = WorkCalendar.Today(clock);
        return Result.Success<IReadOnlyList<WorkTaskDto>>(tasks.Select(task => WorkTaskDto.From(task, today)).ToList());
    }
}
