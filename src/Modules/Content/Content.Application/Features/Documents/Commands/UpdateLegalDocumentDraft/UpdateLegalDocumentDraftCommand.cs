using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Commands.UpdateLegalDocumentDraft;

public sealed record UpdateLegalDocumentDraftCommand(long Id, IReadOnlyList<LegalDocumentTextData> Texts)
    : ICommand<Result<LegalDocumentVersionDetails>>;
