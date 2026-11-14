namespace Work.Application.Features.Tasks.Commands.DeleteMyWorkTask;

public sealed record DeleteMyWorkTaskCommand(long Id) : ICommand<Result>;
