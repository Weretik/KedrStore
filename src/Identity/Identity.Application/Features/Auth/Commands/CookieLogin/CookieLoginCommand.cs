using Identity.Application.Features.Auth.Queries.SessionMe;

namespace Identity.Application.Features.Auth.Commands.CookieLogin;

public sealed record CookieLoginCommand(string Email, string Password) : ICommand<Result<SessionMeResult>>;
