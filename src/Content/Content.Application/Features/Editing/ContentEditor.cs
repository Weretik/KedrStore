using BuildingBlocks.Application.Contracts;

namespace Content.Application.Features.Editing;

/// <summary>Who is saving a change, as stored with it and shown in the admin: email, else user name, else id.</summary>
internal static class ContentEditor
{
    public static string Name(ICurrentUserService user) =>
        new[] { user.Email, user.UserName, user.UserId }.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
        ?? "unknown";
}
