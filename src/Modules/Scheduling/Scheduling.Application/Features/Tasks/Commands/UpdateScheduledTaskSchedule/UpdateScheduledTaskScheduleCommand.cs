using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;

public sealed record ScheduledTaskScheduleInput(bool Enabled, ScheduleInput Schedule, DateTimeOffset? NextRunAt = null);

public sealed record UpdateScheduledTaskScheduleCommand(string Key, ScheduledTaskScheduleInput Input, string Actor)
    : ICommand<Result<ScheduledTaskResponse>>;
