using BuildingBlocks.Application.Contracts;
using Work.Application.Abstractions.Users;

namespace Work.Application.Features.Tasks.Shared;

/// <summary>R-2: a staff member works only with their own tasks; another person's task is "not found".</summary>
internal static class OwnWorkTask
{
    public static async Task<Result<WorkTask>> FindAsync(
        IWorkRepository<WorkTask> tasks, ICurrentUserService user, long id, CancellationToken cancellationToken)
    {
        if (WorkViewer.UserId(user) is not { } viewer)
            return Result<WorkTask>.Unauthorized();

        var task = await tasks.GetByIdAsync(WorkTaskId.Create(id), cancellationToken);
        return task is not null && task.OwnerUserId == viewer
            ? Result.Success(task)
            : Result<WorkTask>.NotFound();
    }
}
