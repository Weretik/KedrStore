using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Media.Api.Endpoints.Files.DeleteMediaFile;

using Media.Api.Endpoints.Files.ReplaceMediaFile;
using Media.Api.Endpoints.Files.UploadMediaFile;
using Media.Api.Endpoints.Files.MoveMediaFiles;
using Media.Api.Endpoints.Files.GetMediaFiles;
using Media.Api.Endpoints.Files.GetMediaFile;
using Media.Api.Endpoints.Files.UpdateMediaFile;

namespace Media.Api.Endpoints.Files;

/// <summary>Pictures of the media library on the CDN. Delete is for Admins only.</summary>
public static class FilesEndpoints
{
    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("api/admin/media/files")
            .WithTags("AdminMediaFiles")
            .RequireAuthorization(PolicyNames.CanManageMedia);
        GetMediaFilesEndpoint.Map(admin);
        GetMediaFileEndpoint.Map(admin);
        UploadMediaFileEndpoint.Map(admin);
        UpdateMediaFileEndpoint.Map(admin);
        ReplaceMediaFileEndpoint.Map(admin);
        MoveMediaFilesEndpoint.Map(admin);
        DeleteMediaFileEndpoint.Map(admin);

        return endpoints;
    }
}
