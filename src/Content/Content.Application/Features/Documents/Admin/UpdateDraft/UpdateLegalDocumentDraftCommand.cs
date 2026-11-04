using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.UpdateDraft;

public sealed record UpdateLegalDocumentDraftCommand(long Id, IReadOnlyList<LegalDocumentTextData> Texts)
    : ICommand<Result<LegalDocumentVersionDetails>>;
