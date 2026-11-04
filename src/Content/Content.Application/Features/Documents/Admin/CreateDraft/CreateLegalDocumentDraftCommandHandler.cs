using BuildingBlocks.Application.Contracts;
using Content.Application.Contracts.Html;
using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.CreateDraft;

/// <summary>
/// One draft per kind: a second one answers 409. Without texts the draft copies the published version, so an
/// edit starts from what the site shows.
/// </summary>
public sealed class CreateLegalDocumentDraftCommandHandler(
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
        var current = await db.LegalDocumentVersions.AsNoTracking()
            .Where(v => v.Kind == command.Kind && v.Status != LegalDocumentStatus.Archived)
            .ToListAsync(cancellationToken);
        if (current.Any(v => v.Status == LegalDocumentStatus.Draft))
            return Result.Conflict("A draft of this document already exists.");

        var texts = command.Texts ?? current.FirstOrDefault(v => v.Status == LegalDocumentStatus.Published)?.Texts
            .Select(text => new LegalDocumentTextData(text.Language, text.Title, text.BodyHtml)).ToList();
        if (texts is null)
            return Result.Invalid(new ValidationError("texts",
                "Nothing is published yet, so the first draft needs both texts.", "Content.Document.Text.Missing",
                ValidationSeverity.Error));

        var draft = LegalDocumentVersion.CreateDraft(command.Kind, LegalDocumentEditor.Sanitized(texts, sanitizer),
            clock.GetUtcNow(), LegalDocumentEditor.Name(currentUser));
        await versions.AddAsync(draft, cancellationToken);
        return Result.Success(LegalDocumentMapper.ToDetails(draft));
    }
}
