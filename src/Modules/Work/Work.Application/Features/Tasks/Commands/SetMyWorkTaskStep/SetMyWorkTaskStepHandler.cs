using BuildingBlocks.Application.Users;
using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.SetMyWorkTaskStep;

public sealed class SetMyWorkTaskStepHandler(
    IWorkRepository<WorkTask> tasks, ICurrentUserService user, TimeProvider clock)
    : ICommandHandler<SetMyWorkTaskStepCommand, Result<WorkTaskDto>>
{
    public async ValueTask<Result<WorkTaskDto>> Handle(SetMyWorkTaskStepCommand command,
        CancellationToken cancellationToken)
    {
        var today = WorkCalendar.Today(clock);
        var found = await OwnWorkTask.FindAsync(tasks, user, command.Id, cancellationToken);
        if (!found.IsSuccess)
            return found.Map(task => WorkTaskDto.From(task, today));

        var task = found.Value;
        if (!task.SetStepDone(command.Index, command.IsDone, clock.GetUtcNow()))
            return Result<WorkTaskDto>.NotFound();

        await tasks.UpdateAsync(task, cancellationToken);
        return Result.Success(WorkTaskDto.From(task, today));
    }
}
