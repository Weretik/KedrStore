namespace Content.Application.Features.Documents.Admin.DeleteDraft;

public sealed record DeleteLegalDocumentDraftCommand(long Id) : ICommand<Result>;
