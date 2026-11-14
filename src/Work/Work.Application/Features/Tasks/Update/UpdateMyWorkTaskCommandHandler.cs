using BuildingBlocks.Application.Contracts;
using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.Update;

public sealed class UpdateMyWorkTaskCommandHandler(
    IWorkRepository<WorkTask> tasks, ICurrentUserService user, TimeProvider clock)
    : ICommandHandler<UpdateMyWorkTaskCommand, Result<WorkTaskDto>>
{
    public async ValueTask<Result<WorkTaskDto>> Handle(UpdateMyWorkTaskCommand command,
        CancellationToken cancellationToken)
    {
        var found = await OwnWorkTask.FindAsync(tasks, user, command.Id, cancellationToken);
        if (!found.IsSuccess)
            return found.Map(task => WorkTaskDto.From(task, WorkCalendar.Today(clock)));

        var task = found.Value;
        var edited = task.Edit(command.Title, command.Details, command.DueDate, command.Steps, clock.GetUtcNow());
        if (edited.IsFailure)
            return edited.Error!.ToResult<WorkTaskDto>();
        await tasks.UpdateAsync(task, cancellationToken);
        return Result.Success(WorkTaskDto.From(task, WorkCalendar.Today(clock)));
    }
}
