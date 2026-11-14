using Identity.Contracts.Authorization;
using Media.Application.Features.Files.Commands.DeleteMediaFile;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Media.Api.Endpoints.Files.DeleteMediaFile;

internal static class DeleteMediaFileEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:long}", Handle)
            .WithName("deleteAdminMediaFile")
            .RequireAuthorization(PolicyNames.RequireAdminRole)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status503ServiceUnavailable);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteMediaFileCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
