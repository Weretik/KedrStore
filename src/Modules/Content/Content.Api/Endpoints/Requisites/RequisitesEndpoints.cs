using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Requisites.GetAdminCompanyRequisitesHistory;

using Content.Api.Endpoints.Requisites.GetCompanyRequisites;
using Content.Api.Endpoints.Requisites.GetAdminCompanyRequisites;
using Content.Api.Endpoints.Requisites.UpdateAdminCompanyRequisites;

namespace Content.Api.Endpoints.Requisites;

/// <summary>
/// Admin editing of the company requisites; Admin role only, because a wrong IBAN costs money. Public company
/// requisites shown on the site requisites page.
/// </summary>
public static class RequisitesEndpoints
{
    public static IEndpointRouteBuilder MapRequisitesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminCompanyRequisites = endpoints.MapGroup("api/admin/content/requisites")
            .WithTags("AdminCompanyRequisites")
            .RequireAuthorization(PolicyNames.RequireAdminRole);
        GetAdminCompanyRequisitesEndpoint.Map(adminCompanyRequisites);
        UpdateAdminCompanyRequisitesEndpoint.Map(adminCompanyRequisites);
        GetAdminCompanyRequisitesHistoryEndpoint.Map(adminCompanyRequisites);

        var companyRequisites = endpoints.MapGroup("api/content/requisites")
            .WithTags("CompanyRequisites")
            .AllowAnonymous();
        GetCompanyRequisitesEndpoint.Map(companyRequisites);

        return endpoints;
    }
}
