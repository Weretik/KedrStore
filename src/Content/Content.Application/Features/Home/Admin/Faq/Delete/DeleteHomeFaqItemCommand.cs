using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Delete;

public sealed record DeleteHomeFaqItemCommand(long Id) : ICommand<Result<AdminHomeBlocks>>;
