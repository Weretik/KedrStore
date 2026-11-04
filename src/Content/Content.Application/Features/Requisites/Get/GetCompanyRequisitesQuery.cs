using Content.Application.Features.Requisites.Common;

namespace Content.Application.Features.Requisites.Get;

/// <summary>The one requisites record; the site and the admin read the same values.</summary>
public sealed record GetCompanyRequisitesQuery : IQuery<Result<CompanyRequisitesDetails>>;
