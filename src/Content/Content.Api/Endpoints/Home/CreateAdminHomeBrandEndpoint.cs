using Content.Application.Features.Home.Commands.CreateHomeBrand;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Home;

internal static class CreateAdminHomeBrandEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("brands", Handle)
            .WithName("createAdminHomeBrand")
            .Produces<AdminHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    /// <summary>Adds a brand to the end; its logo must be a media library file.</summary>
    private static async Task<IResult> Handle(
        [FromBody] HomeBrandRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new CreateHomeBrandCommand(HomeResponseMapper.ToInput(request)), cancellationToken))
            .Map(HomeResponseMapper.ToResponse)
            .ToHttpResult(http);
}
