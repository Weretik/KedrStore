using Content.Application.Features.Contacts.Common;

namespace Content.Application.Features.Contacts.Get;

/// <summary>The one contacts record; the site and the admin read the same values.</summary>
public sealed record GetCompanyContactsQuery : IQuery<Result<CompanyContactsDetails>>;
