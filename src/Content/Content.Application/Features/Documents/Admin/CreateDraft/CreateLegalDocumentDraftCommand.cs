using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.CreateDraft;

/// <summary>Starts a new edition from the published texts (or from <see cref="Texts"/> when nothing is published).</summary>
public sealed record CreateLegalDocumentDraftCommand(LegalDocumentKind Kind, IReadOnlyList<LegalDocumentTextData>? Texts)
    : ICommand<Result<LegalDocumentVersionDetails>>;
