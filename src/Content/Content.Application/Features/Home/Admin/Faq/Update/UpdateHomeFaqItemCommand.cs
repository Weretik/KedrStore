using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Update;

public sealed record UpdateHomeFaqItemCommand(long Id, HomeFaqInput Texts) : ICommand<Result<AdminHomeBlocks>>;
