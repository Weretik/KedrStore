using Identity.Api.Endpoints.Avatars;
using Identity.Application.Features.Profile.Commands.UploadOwnAvatar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Identity.Api.Endpoints.Profile.UploadMyAvatar;

internal static class UploadMyAvatarEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("avatar", Handle)
            .WithName("uploadMyAvatar")
            .AsAvatarUpload()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        IFormFile? file, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => AvatarUploads.ToHttpResult(
            await sender.Send(new UploadOwnAvatarCommand(AvatarUploads.ToUpload(file)), cancellationToken), http);
}
