using BuildingBlocks.Application.Contracts;
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
        var version = await versions.GetByIdAsync(command.Id, cancellationToken);
        if (version is null)
            return Result.NotFound();
        if (!version.IsDraft)
            return Result.Conflict("Only a draft can be changed; start a new draft to edit a published version.");

        version.UpdateDraft(LegalDocumentEditor.Sanitized(command.Texts, sanitizer), clock.GetUtcNow(),
            ContentEditor.Name(currentUser));
        await versions.UpdateAsync(version, cancellationToken);
        return Result.Success(LegalDocumentMapper.ToDetails(version));
    }
}
