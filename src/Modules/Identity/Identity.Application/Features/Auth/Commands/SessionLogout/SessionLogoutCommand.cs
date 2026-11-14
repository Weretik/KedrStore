namespace Identity.Application.Features.Auth.Commands.SessionLogout;

public sealed record SessionLogoutCommand(string? RefreshToken) : ICommand<Result>;
