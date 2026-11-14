using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.DeleteHomeFaqItem;

public sealed record DeleteHomeFaqItemCommand(long Id) : ICommand<Result<AdminHomeBlocks>>;
