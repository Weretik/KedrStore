using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.SetStep;

/// <summary>R-9: ticks or unticks one step of an own task; <paramref name="Index"/> is its position from 0.</summary>
public sealed record SetMyWorkTaskStepCommand(long Id, int Index, bool IsDone) : ICommand<Result<WorkTaskDto>>;
