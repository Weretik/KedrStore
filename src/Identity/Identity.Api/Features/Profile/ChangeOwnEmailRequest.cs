namespace Identity.Api.Features.Profile;

public sealed record ChangeOwnEmailRequest(string Email, string CurrentPassword);
