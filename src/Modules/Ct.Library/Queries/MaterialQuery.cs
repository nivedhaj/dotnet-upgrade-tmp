using Ct.Library.Postgres.ReadModels;
using Marten;
using Microsoft.Extensions.Logging;

namespace Ct.Library.Postgres.Queries;

public sealed record MaterialSearchRequest(
    string InstId,
    string? Search = null,
    int Skip = 0,
    int Take = 50);

public sealed record MaterialViewDto(
    string Id,
    string TrackingId,
    string Title,
    string? Author,
    string? Publisher,
    string? Location,
    string? ISBN,
    string? OwnerId,
    string Status,
    bool CanCirculate);

public sealed record PagedResult<T>(IReadOnlyList<T> Data, int TotalRecords, int RecordsPerPage);

/// <summary>
/// Production query service replacing the original Raven MaterialQry.
/// </summary>
public sealed class MaterialQuery(
    IQuerySession session,
    ILogger<MaterialQuery> logger)
{
    public async Task<PagedResult<MaterialViewDto>> SearchAsync(
        MaterialSearchRequest request,
        CancellationToken ct = default)
    {
        var query = session.Query<MaterialView>()
            .Where(m => m.OwnerId == request.InstId); // multi-tenancy filter

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            // Postgres ILIKE / StartsWith – good enough for catalog search.
            // For richer full-text later: use to_tsvector / Marten full-text helpers.
            query = query.Where(m =>
                m.Title.StartsWith(term, StringComparison.OrdinalIgnoreCase) ||
                (m.TrackingId != null && m.TrackingId.StartsWith(term, StringComparison.OrdinalIgnoreCase)) ||
                (m.ISBN != null && m.ISBN.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(m => m.TrackingId)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(ct);

        var dtos = items.Select(m => new MaterialViewDto(
            m.Id,
            m.TrackingId,
            m.Title,
            m.Author,
            m.Publisher,
            m.Location,
            m.ISBN,
            m.OwnerId,
            m.Status,
            CanCirculate: string.Equals(m.OwnerId, request.InstId, StringComparison.Ordinal)
        )).ToList();

        logger.LogDebug("Material search for InstId={InstId} returned {Count}/{Total}",
            request.InstId, dtos.Count, total);

        return new PagedResult<MaterialViewDto>(dtos, total, dtos.Count);
    }

    public async Task<MaterialViewDto?> GetByIdAsync(string id, string instId, CancellationToken ct = default)
    {
        var view = await session.LoadAsync<MaterialView>(id, ct);
        if (view is null || view.OwnerId != instId)
            return null;

        return new MaterialViewDto(
            view.Id,
            view.TrackingId,
            view.Title,
            view.Author,
            view.Publisher,
            view.Location,
            view.ISBN,
            view.OwnerId,
            view.Status,
            CanCirculate: true);
    }
}
