using BuildingBlocks.Application.Users;
using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.DeleteMyWorkTask;

public sealed class DeleteMyWorkTaskHandler(IWorkRepository<WorkTask> tasks, ICurrentUserService user)
    : ICommandHandler<DeleteMyWorkTaskCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteMyWorkTaskCommand command, CancellationToken cancellationToken)
    {
        var found = await OwnWorkTask.FindAsync(tasks, user, command.Id, cancellationToken);
        if (!found.IsSuccess)
            return found.Status == ResultStatus.Unauthorized ? Result.Unauthorized() : Result.NotFound();

        await tasks.DeleteAsync(found.Value, cancellationToken);
        return Result.Success();
    }
}
