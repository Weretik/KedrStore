namespace Identity.Api.Endpoints.Profile.ChangeMyPassword;

public sealed record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);
