using BuildingBlocks.Application.Users;
using Content.Application.Abstractions.Integrations;
using Content.Application.Features.Documents.Shared;
using Content.Application.Shared;

namespace Content.Application.Features.Documents.Commands.UpdateLegalDocumentDraft;

/// <summary>Replaces both texts of a draft; a published or archived version answers 409.</summary>
public sealed class UpdateLegalDocumentDraftHandler(
    IContentRepository<LegalDocumentVersion> versions,
    IArticleHtmlSanitizer sanitizer,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateLegalDocumentDraftCommand, Result<LegalDocumentVersionDetails>>
{
    public async ValueTask<Result<LegalDocumentVersionDetails>> Handle(UpdateLegalDocumentDraftCommand command,
        CancellationToken cancellationToken)
    {
        var version = await versions.GetByIdAsync(LegalDocumentVersionId.FromStorage(command.Id), cancellationToken);
        if (version is null)
            return Result.NotFound();

        var updated = version.UpdateDraft(LegalDocumentEditor.Sanitized(command.Texts, sanitizer), clock.GetUtcNow(),
            ContentEditor.Name(currentUser));
        if (updated.IsFailure)
            return updated.Error!.ToResult<LegalDocumentVersionDetails>();
        await versions.UpdateAsync(version, cancellationToken);
        await versions.SaveChangesAsync(cancellationToken);
        return Result.Success(LegalDocumentMapper.ToDetails(version));
    }
}
