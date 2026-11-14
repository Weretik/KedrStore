using BuildingBlocks.Application.Users;
using Content.Application.Shared;
using Content.Application.Features.Requisites.Shared;

namespace Content.Application.Features.Requisites.Commands.UpdateCompanyRequisites;

/// <summary>
/// Replaces all requisites and, when any field changed, logs who changed what in the same save. Saving the
/// same values again changes nothing and adds no log entry.
/// </summary>
public sealed class UpdateCompanyRequisitesHandler(
    IContentRepository<CompanyRequisites> requisitesRepository,
    IContentRepository<CompanyRequisitesChange> changesRepository,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateCompanyRequisitesCommand, Result<CompanyRequisitesDetails>>
{
    public async ValueTask<Result<CompanyRequisitesDetails>> Handle(UpdateCompanyRequisitesCommand command,
        CancellationToken cancellationToken)
    {
        var requisites = await requisitesRepository.GetByIdAsync(CompanyRequisites.SingletonId, cancellationToken);
        if (requisites is null)
            return Result.NotFound();

        var now = clock.GetUtcNow();
        var updated = requisites.Update(CompanyRequisitesMapper.ToValues(command.Input), now);
        if (updated.IsFailure)
            return updated.Error!.ToResult<CompanyRequisitesDetails>();
        var changes = updated.Value;
        if (changes.Count == 0)
            return Result.Success(CompanyRequisitesMapper.ToDetails(requisites));

        var userName = ContentEditor.Name(currentUser);
        // One SaveChanges writes the new values and their log entry together.
        await changesRepository.AddAsync(
            CompanyRequisitesChange.Record(changes, now, currentUser.UserId, userName), cancellationToken);
        return Result.Success(CompanyRequisitesMapper.ToDetails(requisites));
    }
}
