using BuildingBlocks.Application.Contracts;
using Content.Application.Contracts.Html;
using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.UpdateDraft;

/// <summary>Replaces both texts of a draft; a published or archived version answers 409.</summary>
public sealed class UpdateLegalDocumentDraftCommandHandler(
    IContentRepository<LegalDocumentVersion> versions,
    IArticleHtmlSanitizer sanitizer,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateLegalDocumentDraftCommand, Result<LegalDocumentVersionDetails>>
{
    public async ValueTask<Result<LegalDocumentVersionDetails>> Handle(UpdateLegalDocumentDraftCommand command,
        CancellationToken cancellationToken)
    {
        var version = await versions.GetByIdAsync(command.Id, cancellationToken);
        if (version is null)
            return Result.NotFound();
        if (version.Status != LegalDocumentStatus.Draft)
            return Result.Conflict("Only a draft can be changed; start a new draft to edit a published version.");

        version.UpdateDraft(LegalDocumentEditor.Sanitized(command.Texts, sanitizer), clock.GetUtcNow(),
            LegalDocumentEditor.Name(currentUser));
        await versions.UpdateAsync(version, cancellationToken);
        return Result.Success(LegalDocumentMapper.ToDetails(version));
    }
}
