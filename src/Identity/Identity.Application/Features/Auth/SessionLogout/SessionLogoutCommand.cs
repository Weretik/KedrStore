namespace Identity.Application.Features.Auth.SessionLogout;

public sealed record SessionLogoutCommand(string? RefreshToken) : ICommand<Result>;
