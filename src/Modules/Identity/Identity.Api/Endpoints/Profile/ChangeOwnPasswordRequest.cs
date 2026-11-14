namespace Identity.Api.Endpoints.Profile;

public sealed record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);
