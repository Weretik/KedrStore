using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Gallery.GetPublicGallery;

using Content.Api.Endpoints.Gallery.DeleteAdminGalleryItem;
using Content.Api.Endpoints.Gallery.GetAdminGallery;
using Content.Api.Endpoints.Gallery.PreviewAdminGalleryYoutubeVideo;
using Content.Api.Endpoints.Gallery.ReorderAdminGallery;
using Content.Api.Endpoints.Gallery.UpdateAdminGalleryItem;
using Content.Api.Endpoints.Gallery.AddAdminGalleryVideo;
using Content.Api.Endpoints.Gallery.AddAdminGalleryPhotos;

namespace Content.Api.Endpoints.Gallery;

/// <summary>
/// Admin editing of the site gallery. Every change answers with the whole gallery in site order, so the page needs
/// no second request. The public site gallery: photos from the media library and YouTube videos, in site order.
/// </summary>
public static class GalleryEndpoints
{
    public static IEndpointRouteBuilder MapGalleryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminGallery = endpoints.MapGroup("api/admin/content/gallery")
            .WithTags("AdminGallery")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminGalleryEndpoint.Map(adminGallery);
        AddAdminGalleryPhotosEndpoint.Map(adminGallery);
        AddAdminGalleryVideoEndpoint.Map(adminGallery);
        PreviewAdminGalleryYoutubeVideoEndpoint.Map(adminGallery);
        UpdateAdminGalleryItemEndpoint.Map(adminGallery);
        DeleteAdminGalleryItemEndpoint.Map(adminGallery);
        ReorderAdminGalleryEndpoint.Map(adminGallery);

        var gallery = endpoints.MapGroup("api/content")
            .WithTags("Gallery")
            .AllowAnonymous();
        GetPublicGalleryEndpoint.Map(gallery);

        return endpoints;
    }
}
