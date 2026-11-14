using Content.Application.Features.Wholesale.Commands.UpdateWholesalePage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Wholesale.Shared;

namespace Content.Api.Endpoints.Wholesale.UpdateAdminWholesalePage;

internal static class UpdateAdminWholesalePageEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("", Handle)
            .WithName("updateAdminWholesalePage")
            .Produces<AdminWholesalePageResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromBody] SaveWholesalePageRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateWholesalePageCommand(WholesaleResponseMapper.ToInput(request)),
            cancellationToken);
        return result.Map(WholesaleResponseMapper.ToResponse).ToHttpResult(http);
    }
}
