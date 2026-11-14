namespace Identity.Api.Features.Profile;

public sealed record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);
