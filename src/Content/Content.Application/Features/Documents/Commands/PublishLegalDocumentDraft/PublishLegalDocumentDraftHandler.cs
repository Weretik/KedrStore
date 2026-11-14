using BuildingBlocks.Application.Contracts;
using Content.Application.Features.Documents.Shared;
using Content.Application.Shared;

namespace Content.Application.Features.Documents.Commands.PublishLegalDocumentDraft;

/// <summary>
/// Publishes a draft as the next number and archives the version it replaces, in one save. The «Редакція від»
/// date may not be earlier than the replaced version's date.
/// </summary>
public sealed class PublishLegalDocumentDraftHandler(
    IContentRepository<LegalDocumentVersion> versions,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<PublishLegalDocumentDraftCommand, Result<LegalDocumentVersionDetails>>
{
    public async ValueTask<Result<LegalDocumentVersionDetails>> Handle(PublishLegalDocumentDraftCommand command,
        CancellationToken cancellationToken)
    {
        var draft = await versions.GetByIdAsync(command.Id, cancellationToken);
        if (draft is null)
            return Result.NotFound();
        if (!draft.IsDraft)
            return Result.Conflict("This version is already published.");

        var kind = draft.Kind;
        var published = await versions.FirstOrDefaultAsync(
            new LegalDocumentVersionByStatusSpec(kind, LegalDocumentStatus.Published), cancellationToken);
        if (!LegalDocumentVersion.AllowsEffectiveFrom(command.EffectiveFrom, published))
        {
            var error = LegalDocumentErrors.EffectiveFromTooEarly(published!.EffectiveFrom!.Value);
            return Result.Invalid(new ValidationError("effectiveFrom", error.Message, error.Code,
                ValidationSeverity.Error));
        }

        var lastNumber = await versions.ListAsync(new LegalDocumentNumbersSpec(kind), cancellationToken);
        draft.PublishReplacing(published, lastNumber.Count == 0 ? 0 : lastNumber.Max(), command.EffectiveFrom,
            clock.GetUtcNow(), ContentEditor.Name(currentUser));
        await versions.SaveChangesAsync(cancellationToken);
        return Result.Success(LegalDocumentMapper.ToDetails(draft));
    }
}
