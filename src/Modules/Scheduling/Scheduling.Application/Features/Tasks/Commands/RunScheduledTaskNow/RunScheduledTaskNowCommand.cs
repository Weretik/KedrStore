using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;

public sealed record RunScheduledTaskNowCommand(string Key, string Actor) : ICommand<Result<ScheduledTaskResponse>>;
