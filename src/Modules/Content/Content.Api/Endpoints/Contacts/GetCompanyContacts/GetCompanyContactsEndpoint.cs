using Content.Application.Features.Contacts.Queries.GetCompanyContacts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Contacts.Shared;

namespace Content.Api.Endpoints.Contacts.GetCompanyContacts;

internal static class GetCompanyContactsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getCompanyContacts")
            .Produces<CompanyContactsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyContactsQuery(), cancellationToken);
        return result.Map(CompanyContactsResponseMapper.ToResponse).ToHttpResult(http);
    }
}
