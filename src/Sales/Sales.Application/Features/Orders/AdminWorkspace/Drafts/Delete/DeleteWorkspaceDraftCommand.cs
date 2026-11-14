namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.Delete;

public sealed record DeleteWorkspaceDraftCommand(long DraftId, string Version) : ICommand<Result>;
