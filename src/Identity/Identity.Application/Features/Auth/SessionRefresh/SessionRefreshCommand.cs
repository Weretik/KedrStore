using Identity.Application.Features.Auth.SessionRefresh.DTOs;

namespace Identity.Application.Features.Auth.SessionRefresh;

public sealed record SessionRefreshCommand(SessionRefreshRequest Request) : ICommand<Result<SessionRefreshResult>>;
