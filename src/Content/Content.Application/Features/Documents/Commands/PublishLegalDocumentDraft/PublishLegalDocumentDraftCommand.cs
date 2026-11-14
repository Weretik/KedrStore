using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Commands.PublishLegalDocumentDraft;

/// <summary>Puts a draft on the site with its «Редакція від» date.</summary>
public sealed record PublishLegalDocumentDraftCommand(long Id, DateOnly EffectiveFrom)
    : ICommand<Result<LegalDocumentVersionDetails>>;
