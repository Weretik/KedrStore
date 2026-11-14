using Content.Application.Features.Contacts.Queries.GetCompanyContacts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Contacts.Shared;

namespace Content.Api.Endpoints.Contacts.GetAdminCompanyContacts;

internal static class GetAdminCompanyContactsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminCompanyContacts")
            .Produces<AdminCompanyContactsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyContactsQuery(), cancellationToken);
        return result.Map(CompanyContactsResponseMapper.ToAdminResponse).ToHttpResult(http);
    }
}
