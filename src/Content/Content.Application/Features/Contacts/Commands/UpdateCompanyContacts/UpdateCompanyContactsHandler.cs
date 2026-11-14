using BuildingBlocks.Application.Contracts;
using Content.Application.Shared;
using Content.Application.Features.Contacts.Shared;

namespace Content.Application.Features.Contacts.Commands.UpdateCompanyContacts;

/// <summary>Replaces all contacts at once; the row comes from the migration, so a missing one is 404.</summary>
public sealed class UpdateCompanyContactsHandler(
    IContentRepository<CompanyContacts> repository,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateCompanyContactsCommand, Result<CompanyContactsDetails>>
{
    public async ValueTask<Result<CompanyContactsDetails>> Handle(UpdateCompanyContactsCommand command,
        CancellationToken cancellationToken)
    {
        var contacts = await repository.GetByIdAsync(CompanyContacts.SingletonId, cancellationToken);
        if (contacts is null)
            return Result.NotFound();

        var updated = contacts.Update(CompanyContactsMapper.ToValues(command.Input), clock.GetUtcNow(),
            ContentEditor.Name(currentUser));
        if (updated.IsFailure)
            return updated.Error!.ToResult<CompanyContactsDetails>();
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(CompanyContactsMapper.ToDetails(contacts));
    }
}
