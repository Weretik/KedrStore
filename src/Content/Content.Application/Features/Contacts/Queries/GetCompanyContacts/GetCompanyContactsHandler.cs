using Content.Application.Features.Contacts.Shared;

namespace Content.Application.Features.Contacts.Queries.GetCompanyContacts;

public sealed class GetCompanyContactsHandler(IReadContentDbContext db)
    : IQueryHandler<GetCompanyContactsQuery, Result<CompanyContactsDetails>>
{
    public async ValueTask<Result<CompanyContactsDetails>> Handle(GetCompanyContactsQuery query,
        CancellationToken cancellationToken)
    {
        var contacts = await db.CompanyContacts.AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == CompanyContacts.SingletonId, cancellationToken);
        return contacts is null ? Result.NotFound() : Result.Success(CompanyContactsMapper.ToDetails(contacts));
    }
}
