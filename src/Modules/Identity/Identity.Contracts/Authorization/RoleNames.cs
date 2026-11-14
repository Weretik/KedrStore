namespace Identity.Contracts.Authorization;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Manager = "Manager";

    /// <summary>SEO specialist: everything in the admin except the «Система» section (users, requisites).</summary>
    public const string Seo = "Seo";
    public const string Guest = "Guest";

    public static readonly IReadOnlyList<string> All =
    [
        Admin,
        Seo,
        Manager,
        User,
        Guest
    ];
}
