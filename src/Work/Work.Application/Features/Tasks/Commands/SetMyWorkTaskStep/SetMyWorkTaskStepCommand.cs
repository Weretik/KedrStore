using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.SetMyWorkTaskStep;

/// <summary>R-9: ticks or unticks one step of an own task; <paramref name="Index"/> is its position from 0.</summary>
public sealed record SetMyWorkTaskStepCommand(long Id, int Index, bool IsDone) : ICommand<Result<WorkTaskDto>>;
