using BuildingBlocks.Application.Contracts;
using Content.Application.Abstractions.Integrations;
using Content.Application.Features.Documents.Shared;
using Content.Application.Shared;

namespace Content.Application.Features.Documents.Commands.CreateLegalDocumentDraft;

/// <summary>
/// One draft per kind: a second one answers 409. Without texts the draft copies the published version, so an
/// edit starts from what the site shows.
/// </summary>
public sealed class CreateLegalDocumentDraftHandler(
    IContentRepository<LegalDocumentVersion> versions,
    IReadContentDbContext db,
    IArticleHtmlSanitizer sanitizer,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<CreateLegalDocumentDraftCommand, Result<LegalDocumentVersionDetails>>
{
    public async ValueTask<Result<LegalDocumentVersionDetails>> Handle(CreateLegalDocumentDraftCommand command,
        CancellationToken cancellationToken)
    {
        if (!LegalDocumentKinds.TryParse(command.Kind, out var kind))
            return Result.NotFound();

        var current = await db.LegalDocumentVersions.AsNoTracking()
            .Where(v => v.Kind == kind && v.Status != LegalDocumentStatus.Archived)
            .ToListAsync(cancellationToken);
        if (current.Any(v => v.Status == LegalDocumentStatus.Draft))
            return Result.Conflict("A draft of this document already exists.");

        var texts = command.Texts ?? current.FirstOrDefault(v => v.Status == LegalDocumentStatus.Published)?.Texts
            .Select(text => new LegalDocumentTextData(text.Language, text.Title, text.BodyHtml)).ToList();
        if (texts is null)
            return Result.Invalid(new ValidationError("texts",
                "Nothing is published yet, so the first draft needs both texts.", "Content.Document.Text.Missing",
                ValidationSeverity.Error));

        var draft = LegalDocumentVersion.CreateDraft(kind, LegalDocumentEditor.Sanitized(texts, sanitizer),
            clock.GetUtcNow(), ContentEditor.Name(currentUser));
        await versions.AddAsync(draft, cancellationToken);
        return Result.Success(LegalDocumentMapper.ToDetails(draft));
    }
}
