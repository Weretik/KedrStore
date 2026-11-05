using BuildingBlocks.Application.Contracts;
using Catalog.Domain.Entities.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Common;

/// <summary>Who accepted a product, as stored and shown in the admin: email, else user name, else id.</summary>
internal static class ReviewerName
{
    public static string Of(ICurrentUserService user)
    {
        var name = new[] { user.Email, user.UserName, user.UserId }
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? "unknown";
        return name.Length <= ProductAttributeReview.AcceptedByMaxLength
            ? name
            : name[..ProductAttributeReview.AcceptedByMaxLength];
    }
}
