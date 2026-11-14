using Identity.Application.Features.Auth.Commands.SessionLogin;

namespace Identity.Application.Features.Auth.Commands.SessionLogin;

public sealed record SessionLoginCommand(SessionLoginCommandRequest Request) : ICommand<Result<SessionLoginResult>>;
