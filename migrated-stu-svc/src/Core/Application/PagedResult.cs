namespace Ct.Core.Application;

/// <summary>
/// Replacement for the legacy `PagedResultDto` (spec section 54). All
/// pagination must happen at the database level — callers should apply
/// Skip/Take before materializing, never after ToListAsync().
/// </summary>
public class PagedResult<T>
{
    public required int Count { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required IReadOnlyList<T> Items { get; init; }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int count, int page, int pageSize) =>
        new() { Items = items, Count = count, Page = page, PageSize = pageSize };
}
