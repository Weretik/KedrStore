using BuildingBlocks.Application.Contracts;
using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.ChangeMyWorkTaskStatus;

public sealed class ChangeMyWorkTaskStatusHandler(
    IWorkRepository<WorkTask> tasks, ICurrentUserService user, TimeProvider clock)
    : ICommandHandler<ChangeMyWorkTaskStatusCommand, Result<WorkTaskDto>>
{
    public async ValueTask<Result<WorkTaskDto>> Handle(ChangeMyWorkTaskStatusCommand command,
        CancellationToken cancellationToken)
    {
        var found = await OwnWorkTask.FindAsync(tasks, user, command.Id, cancellationToken);
        if (!found.IsSuccess)
            return found.Map(task => WorkTaskDto.From(task, WorkCalendar.Today(clock)));

        if (!WorkTaskStatusNames.TryParse(command.Status, out var status))
            return Result<WorkTaskDto>.Invalid(new ValidationError(nameof(command.Status), "Невідомий статус."));

        var task = found.Value;
        var changed = task.ChangeStatus(status, command.Result, clock.GetUtcNow());
        if (changed.IsFailure)
            return changed.Error!.ToResult<WorkTaskDto>();
        await tasks.UpdateAsync(task, cancellationToken);
        return Result.Success(WorkTaskDto.From(task, WorkCalendar.Today(clock)));
    }
}
