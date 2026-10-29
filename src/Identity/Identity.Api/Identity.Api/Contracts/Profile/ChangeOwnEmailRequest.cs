namespace Identity.Api.Contracts.Profile;

public sealed record ChangeOwnEmailRequest(string Email, string CurrentPassword);
