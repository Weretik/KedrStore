namespace Identity.Api.Endpoints.Profile;

public sealed record ChangeOwnEmailRequest(string Email, string CurrentPassword);
