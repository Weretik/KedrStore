using Content.Application.Features.Contacts.Commands.UpdateCompanyContacts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Contacts.Shared;

namespace Content.Api.Endpoints.Contacts.UpdateAdminCompanyContacts;

internal static class UpdateAdminCompanyContactsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("", Handle)
            .WithName("updateAdminCompanyContacts")
            .Produces<AdminCompanyContactsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromBody] SaveCompanyContactsRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCompanyContactsCommand(CompanyContactsResponseMapper.ToInput(request)), cancellationToken);
        return result.Map(CompanyContactsResponseMapper.ToAdminResponse).ToHttpResult(http);
    }
}
