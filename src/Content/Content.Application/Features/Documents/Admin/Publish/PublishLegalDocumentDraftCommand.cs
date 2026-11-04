using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.Publish;

/// <summary>Puts a draft on the site with its «Редакція від» date.</summary>
public sealed record PublishLegalDocumentDraftCommand(long Id, DateOnly EffectiveFrom)
    : ICommand<Result<LegalDocumentVersionDetails>>;
