using Identity.Application.Features.Auth.SessionMe.DTOs;

namespace Identity.Application.Features.Auth.CookieLogin;

public sealed record CookieLoginCommand(string Email, string Password) : ICommand<Result<SessionMeResult>>;
