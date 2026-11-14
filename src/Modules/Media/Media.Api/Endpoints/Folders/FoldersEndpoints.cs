using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Media.Api.Endpoints.Folders.GetMediaFolderTree;

using Media.Api.Endpoints.Folders.DeleteMediaFolder;
using Media.Api.Endpoints.Folders.CreateMediaFolder;
using Media.Api.Endpoints.Folders.UpdateMediaFolder;

namespace Media.Api.Endpoints.Folders;

/// <summary>Folders of the media library. Delete is for Admins only.</summary>
public static class FoldersEndpoints
{
    public static IEndpointRouteBuilder MapFoldersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("api/admin/media/folders")
            .WithTags("AdminMediaFolders")
            .RequireAuthorization(PolicyNames.CanManageMedia);
        GetMediaFolderTreeEndpoint.Map(admin);
        CreateMediaFolderEndpoint.Map(admin);
        UpdateMediaFolderEndpoint.Map(admin);
        DeleteMediaFolderEndpoint.Map(admin);

        return endpoints;
    }
}
