using Identity.Application.Features.Auth.SessionLogin.DTOs;

namespace Identity.Application.Features.Auth.SessionLogin;

public sealed record SessionLoginCommand(SessionLoginCommandRequest Request) : ICommand<Result<SessionLoginResult>>;
