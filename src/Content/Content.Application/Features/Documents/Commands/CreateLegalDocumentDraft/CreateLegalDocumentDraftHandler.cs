using BuildingBlocks.Application.Users;
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
            return Result.Conflict("Чернетка цього документа вже є.");

        var texts = command.Texts ?? current.FirstOrDefault(v => v.Status == LegalDocumentStatus.Published)?.Texts
            .Select(text => new LegalDocumentTextData(text.Language, text.Title, text.BodyHtml)).ToList();
        if (texts is null)
            return Result.Invalid(new ValidationError("texts",
                "Документ ще не опубліковано, тож перша чернетка потребує обох текстів.", "Content.Document.Text.Missing",
                ValidationSeverity.Error));

        var created = LegalDocumentVersion.CreateDraft(kind, LegalDocumentEditor.Sanitized(texts, sanitizer),
            clock.GetUtcNow(), ContentEditor.Name(currentUser));
        if (created.IsFailure)
            return created.Error!.ToResult<LegalDocumentVersionDetails>();
        var draft = created.Value;
        await versions.AddAsync(draft, cancellationToken);
        return Result.Success(LegalDocumentMapper.ToDetails(draft));
    }
}
