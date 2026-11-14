namespace Content.Application.Features.Documents.Commands.DeleteLegalDocumentDraft;

public sealed record DeleteLegalDocumentDraftCommand(long Id) : ICommand<Result>;
