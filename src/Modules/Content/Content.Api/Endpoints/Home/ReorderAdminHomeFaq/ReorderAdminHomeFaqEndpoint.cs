using Content.Application.Features.Home.Commands.ReorderHomeFaq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Home.Shared;

namespace Content.Api.Endpoints.Home.ReorderAdminHomeFaq;

internal static class ReorderAdminHomeFaqEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("faq/order", Handle)
            .WithName("reorderAdminHomeFaq")
            .WithSummary("Save the order of all questions")
            .WithDescription("Requires `CanManageContent`. `ids` must hold every current question exactly once, first shown first; a list built from a stale page answers 409.")
            .Produces<AdminHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

    /// <summary>Saves the order of all questions. 409 when the list does not match the current FAQ.</summary>
    private static async Task<IResult> Handle(
        [FromBody] ReorderHomeRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new ReorderHomeFaqCommand(request.Ids ?? []), cancellationToken))
            .Map(HomeResponseMapper.ToResponse)
            .ToHttpResult(http);
}
