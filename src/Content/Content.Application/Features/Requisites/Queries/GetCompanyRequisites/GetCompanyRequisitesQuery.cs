using Content.Application.Features.Requisites.Shared;

namespace Content.Application.Features.Requisites.Queries.GetCompanyRequisites;

/// <summary>The one requisites record; the site and the admin read the same values.</summary>
public sealed record GetCompanyRequisitesQuery : IQuery<Result<CompanyRequisitesDetails>>;
