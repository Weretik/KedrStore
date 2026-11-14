using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Contacts.GetAdminCompanyContacts;

using Content.Api.Endpoints.Contacts.GetCompanyContacts;
using Content.Api.Endpoints.Contacts.UpdateAdminCompanyContacts;

namespace Content.Api.Endpoints.Contacts;

/// <summary>
/// Admin editing of the company contacts shown on the site. Public company contacts that every site page shows.
/// </summary>
public static class ContactsEndpoints
{
    public static IEndpointRouteBuilder MapContactsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminCompanyContacts = endpoints.MapGroup("api/admin/content/contacts")
            .WithTags("AdminCompanyContacts")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminCompanyContactsEndpoint.Map(adminCompanyContacts);
        UpdateAdminCompanyContactsEndpoint.Map(adminCompanyContacts);

        var companyContacts = endpoints.MapGroup("api/content/contacts")
            .WithTags("CompanyContacts")
            .AllowAnonymous();
        GetCompanyContactsEndpoint.Map(companyContacts);

        return endpoints;
    }
}
