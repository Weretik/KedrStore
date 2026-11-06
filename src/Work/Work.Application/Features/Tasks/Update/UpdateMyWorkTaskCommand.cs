using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.Update;

public sealed record UpdateMyWorkTaskCommand(long Id, string Title, string? Details) : ICommand<Result<WorkTaskDto>>;
