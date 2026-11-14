using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Documents.GetAdminLegalDocuments;

using Content.Api.Endpoints.Documents.GetPublishedLegalDocument;
using Content.Api.Endpoints.Documents.GetAdminLegalDocumentVersion;
using Content.Api.Endpoints.Documents.DeleteAdminLegalDocumentDraft;
using Content.Api.Endpoints.Documents.GetAdminLegalDocumentVersions;
using Content.Api.Endpoints.Documents.CreateAdminLegalDocumentDraft;
using Content.Api.Endpoints.Documents.UpdateAdminLegalDocumentDraft;
using Content.Api.Endpoints.Documents.PublishAdminLegalDocumentDraft;

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
