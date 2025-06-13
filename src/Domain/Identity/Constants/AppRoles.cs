namespace Domain.Identity.Constants;
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Manager = "Manager";
    public const string Guest = "Guest";

    public static readonly IReadOnlyList<string> All =
    [
        Admin,
        Manager,
        User,
        Guest
    ];
}

