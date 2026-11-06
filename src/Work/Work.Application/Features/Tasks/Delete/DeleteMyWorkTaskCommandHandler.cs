using BuildingBlocks.Application.Contracts;
using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.Delete;

public sealed class DeleteMyWorkTaskCommandHandler(IWorkRepository<WorkTask> tasks, ICurrentUserService user)
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
