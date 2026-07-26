namespace Identity.Domain.Authorization;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Manager = "Manager";
    public const string Guest = "Guest";
    public const string Counterparty = "Counterparty";

    public static readonly IReadOnlyList<string> All =
    [
        Admin,
        Manager,
        User,
        Guest,
        Counterparty
    ];
}
