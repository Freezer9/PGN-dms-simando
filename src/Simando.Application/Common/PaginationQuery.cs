namespace Simando.Application.Common;

public sealed record PaginationQuery(int Page = 1, int PageSize = 25);
