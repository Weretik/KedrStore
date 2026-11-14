namespace Sales.Application.Features.OrderWorkspace.Commands.DeleteWorkspaceDraft;

public sealed record DeleteWorkspaceDraftCommand(long DraftId, string Version) : ICommand<Result>;
