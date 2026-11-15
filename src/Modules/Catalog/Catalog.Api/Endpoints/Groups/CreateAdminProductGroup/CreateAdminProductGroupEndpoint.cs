using Catalog.Application.Features.Groups.Commands.CreateProductGroup;
using Catalog.Contracts.Groups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Groups.CreateAdminProductGroup;

internal static class CreateAdminProductGroupEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createAdminProductGroup")
            .WithSummary("Create a group («Нова група»)")
            .WithDescription("The listed products (up to 200, all in the category) move into the new group and are pinned: automation never moves them again. The slug is built from the name.")
            .Produces<AdminProductGroupDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromBody] CreateProductGroupRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new CreateProductGroupCommand(request), cancellationToken)).ToHttpResult(http);
}
