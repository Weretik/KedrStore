namespace Identity.Api.Contracts.Profile;

public sealed record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);
