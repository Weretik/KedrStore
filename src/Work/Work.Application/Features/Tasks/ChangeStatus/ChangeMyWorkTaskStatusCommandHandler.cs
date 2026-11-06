using BuildingBlocks.Application.Contracts;
using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.ChangeStatus;

public sealed class ChangeMyWorkTaskStatusCommandHandler(
    IWorkRepository<WorkTask> tasks, ICurrentUserService user, TimeProvider clock)
    : ICommandHandler<ChangeMyWorkTaskStatusCommand, Result<WorkTaskDto>>
{
    public async ValueTask<Result<WorkTaskDto>> Handle(ChangeMyWorkTaskStatusCommand command,
        CancellationToken cancellationToken)
    {
        var found = await OwnWorkTask.FindAsync(tasks, user, command.Id, cancellationToken);
        if (!found.IsSuccess)
            return found.Map(WorkTaskDto.From);

        if (!WorkTaskStatusNames.TryParse(command.Status, out var status))
            return Result<WorkTaskDto>.Invalid(new ValidationError(nameof(command.Status), "Unknown status."));

        var task = found.Value;
        task.ChangeStatus(status, command.Result, clock.GetUtcNow());
        await tasks.UpdateAsync(task, cancellationToken);
        return Result.Success(WorkTaskDto.From(task));
    }
}
