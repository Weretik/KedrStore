using Content.Application.Features.Contacts.Shared;

namespace Content.Application.Features.Contacts.Queries.GetCompanyContacts;

/// <summary>The one contacts record; the site and the admin read the same values.</summary>
public sealed record GetCompanyContactsQuery : IQuery<Result<CompanyContactsDetails>>;
