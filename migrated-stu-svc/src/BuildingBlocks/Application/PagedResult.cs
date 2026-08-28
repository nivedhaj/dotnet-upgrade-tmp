namespace Ct.BuildingBlocks.Application;
public sealed record PagedResult<T>(IReadOnlyList<T> Data, int Page, int PageSize, int TotalRecords)
{
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalRecords / (double)PageSize);
}
