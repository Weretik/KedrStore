namespace Work.Application.Features.Tasks.Delete;

public sealed record DeleteMyWorkTaskCommand(long Id) : ICommand<Result>;
