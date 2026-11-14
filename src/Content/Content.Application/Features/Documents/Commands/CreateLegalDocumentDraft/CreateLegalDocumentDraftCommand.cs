using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Commands.CreateLegalDocumentDraft;

/// <summary>Starts a new edition from the published texts (or from <see cref="Texts"/> when nothing is published).</summary>
public sealed record CreateLegalDocumentDraftCommand(string Kind, IReadOnlyList<LegalDocumentTextData>? Texts)
    : ICommand<Result<LegalDocumentVersionDetails>>;
