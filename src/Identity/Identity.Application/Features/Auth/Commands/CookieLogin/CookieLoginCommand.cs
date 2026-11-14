using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.Commands.CookieLogin;

public sealed record CookieLoginCommand(string Email, string Password) : ICommand<Result<SessionMeResult>>;
