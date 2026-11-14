using Content.Application.Features.Home.Commands.CreateHomeFaqItem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Home;

internal static class CreateAdminHomeFaqItemEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("faq", Handle)
            .WithName("createAdminHomeFaqItem")
            .Produces<AdminHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    /// <summary>Adds a question to the end of the FAQ.</summary>
    private static async Task<IResult> Handle(
        [FromBody] HomeFaqItemRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new CreateHomeFaqItemCommand(HomeResponseMapper.ToInput(request)), cancellationToken))
            .Map(HomeResponseMapper.ToResponse)
            .ToHttpResult(http);
}
