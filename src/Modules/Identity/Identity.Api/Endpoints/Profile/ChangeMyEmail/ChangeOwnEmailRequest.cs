namespace Identity.Api.Endpoints.Profile.ChangeMyEmail;

public sealed record ChangeOwnEmailRequest(string Email, string CurrentPassword);
