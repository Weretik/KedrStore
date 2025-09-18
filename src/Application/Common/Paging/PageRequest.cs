namespace Application.Common.Paging;

public sealed record PageRequest(int CurrentPage = 1, int PageSize = 12);
