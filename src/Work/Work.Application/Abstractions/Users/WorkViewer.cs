using BuildingBlocks.Application.Contracts;

namespace Work.Application.Abstractions.Users;

/// <summary>The signed-in staff member whose board and report a request works with.</summary>
public static class WorkViewer
{
    /// <summary>The user ID from the session, or <c>null</c> when it is missing or not a Guid.</summary>
    public static Guid? UserId(ICurrentUserService user) =>
        Guid.TryParse(user.UserId, out var id) && id != Guid.Empty ? id : null;
}
