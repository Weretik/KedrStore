using Identity.Api.Endpoints.Avatars;
using Identity.Application.Features.Users.Commands.UploadStaffAvatar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Users;

internal static class UploadStaffAvatarEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{userId:guid}/avatar", Handle)
            .WithName("uploadStaffAvatar")
            .AsAvatarUpload()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        Guid userId, IFormFile? file, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => AvatarUploads.ToHttpResult(
            await sender.Send(new UploadStaffAvatarCommand(userId, AvatarUploads.ToUpload(file)), cancellationToken), http);
}
