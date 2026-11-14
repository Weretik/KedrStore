using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Documents;

/// <summary>
/// Admin editing of site documents: one draft per document, publish with «Редакція від», read-only history. Public
/// site documents: only the published edition is shown.
/// </summary>
public static class DocumentsEndpoints
{
    public static IEndpointRouteBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminLegalDocuments = endpoints.MapGroup("api/admin/content/documents")
            .WithTags("AdminLegalDocuments")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminLegalDocumentsEndpoint.Map(adminLegalDocuments);
        GetAdminLegalDocumentVersionsEndpoint.Map(adminLegalDocuments);
        CreateAdminLegalDocumentDraftEndpoint.Map(adminLegalDocuments);
        GetAdminLegalDocumentVersionEndpoint.Map(adminLegalDocuments);
        UpdateAdminLegalDocumentDraftEndpoint.Map(adminLegalDocuments);
        DeleteAdminLegalDocumentDraftEndpoint.Map(adminLegalDocuments);
        PublishAdminLegalDocumentDraftEndpoint.Map(adminLegalDocuments);

        var legalDocuments = endpoints.MapGroup("api/content")
            .WithTags("LegalDocuments")
            .AllowAnonymous();
        GetPublishedLegalDocumentEndpoint.Map(legalDocuments);

        return endpoints;
    }
}
