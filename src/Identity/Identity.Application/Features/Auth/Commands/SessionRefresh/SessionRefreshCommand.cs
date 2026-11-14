using Identity.Application.Features.Auth.Commands.SessionRefresh;

namespace Identity.Application.Features.Auth.Commands.SessionRefresh;

public sealed record SessionRefreshCommand(SessionRefreshRequest Request) : ICommand<Result<SessionRefreshResult>>;
