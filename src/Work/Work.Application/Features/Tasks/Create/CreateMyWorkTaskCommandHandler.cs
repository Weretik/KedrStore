using BuildingBlocks.Application.Contracts;
using Work.Application.Contracts.Users;
using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.Create;

public sealed class CreateMyWorkTaskCommandHandler(
    IWorkRepository<WorkTask> tasks, ICurrentUserService user, TimeProvider clock)
    : ICommandHandler<CreateMyWorkTaskCommand, Result<WorkTaskDto>>
{
    public async ValueTask<Result<WorkTaskDto>> Handle(CreateMyWorkTaskCommand command,
        CancellationToken cancellationToken)
    {
        if (WorkViewer.UserId(user) is not { } viewer)
            return Result<WorkTaskDto>.Unauthorized();

        var task = WorkTask.Create(viewer, viewer, command.Title, command.Details, clock.GetUtcNow());
        await tasks.AddAsync(task, cancellationToken);
        return Result.Success(WorkTaskDto.From(task));
    }
}
