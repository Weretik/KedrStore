using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.UpdateMyWorkTask;

public sealed record UpdateMyWorkTaskCommand(
    long Id, string Title, string? Details, DateOnly? DueDate, IReadOnlyList<WorkTaskStepInput>? Steps)
    : ICommand<Result<WorkTaskDto>>;
