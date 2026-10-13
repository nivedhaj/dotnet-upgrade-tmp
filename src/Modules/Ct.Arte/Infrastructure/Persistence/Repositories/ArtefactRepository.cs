using Ct.Arte.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ct.Arte.Infrastructure.Persistence.Repositories;

public sealed class ArtefactRepository : IArtefactRepository
{
    private readonly ArtefactDbContext _db;
    private readonly ICurrentUser _user;

    public ArtefactRepository(ArtefactDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public Task<Artefact?> GetByIdAsync(string id, CancellationToken ct = default)
        => _db.Artefacts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Artefact?> GetByIdTrackedAsync(string id, CancellationToken ct = default)
        => _db.Artefacts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Artefact>> ListByOwnerAsync(string ownerId, CancellationToken ct = default)
        => await _db.Artefacts.AsNoTracking()
            .Where(a => a.OwnerId == ownerId && a.Status != ArtefactStatus.Disabled)
            .OrderByDescending(a => a.CreatedOn)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Artefact>> SearchAsync(
        string ownerId, string? search, ArtefactStatus? status, int skip, int take, CancellationToken ct = default)
    {
        var query = _db.Artefacts.AsNoTracking().Where(a => a.OwnerId == ownerId);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        else query = query.Where(a => a.Status != ArtefactStatus.Disabled);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(a =>
                a.Title.ToLower().Contains(term) ||
                (a.Description != null && a.Description.ToLower().Contains(term)) ||
                a.Tags.Any(t => t.ToLower().Contains(term)));
        }
        return await query.OrderByDescending(a => a.CreatedOn)
            .Skip(skip).Take(Math.Clamp(take, 1, 500)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Artefact>> GetByTagsAsync(string ownerId, IEnumerable<string> tags, CancellationToken ct = default)
    {
        var tagList = tags.Select(t => t.ToLowerInvariant()).ToList();
        var all = await _db.Artefacts.AsNoTracking()
            .Where(a => a.OwnerId == ownerId && a.Status != ArtefactStatus.Disabled)
            .ToListAsync(ct);
        return all.Where(a => a.Tags.Any(t => tagList.Contains(t.ToLowerInvariant()))).ToList();
    }

    public async Task<IReadOnlyList<Artefact>> GetByMimeTypeAsync(string ownerId, string mimeType, CancellationToken ct = default)
        => await _db.Artefacts.AsNoTracking()
            .Where(a => a.OwnerId == ownerId && a.MimeType == mimeType && a.Status != ArtefactStatus.Disabled)
            .OrderByDescending(a => a.CreatedOn)
            .ToListAsync(ct);

    public Task<Artefact?> GetByFilenameAsync(string ownerId, string fileName, CancellationToken ct = default)
        => _db.Artefacts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.OwnerId == ownerId && a.FileName == fileName, ct);

    public Task<bool> FilenameExistsAsync(string ownerId, string fileName, CancellationToken ct = default)
        => _db.Artefacts.AsNoTracking()
            .AnyAsync(a => a.OwnerId == ownerId && a.FileName == fileName && a.Status != ArtefactStatus.Disabled, ct);

    public async Task AddAsync(Artefact artefact, CancellationToken ct = default)
    {
        artefact.EnsureId();
        if (string.IsNullOrEmpty(artefact.OwnerId) && !string.IsNullOrEmpty(_user.OwnerId))
            artefact.SetOwner(_user.OwnerId!);
        await _db.Artefacts.AddAsync(artefact, ct);
    }

    public Task UpdateAsync(Artefact artefact, CancellationToken ct = default)
    {
        _db.Artefacts.Update(artefact);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _db.Artefacts.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (entity is null) return;
        if (!_user.IsRootAdmin && entity.OwnerId != _user.OwnerId)
            throw new UnauthorizedAccessException("Cannot delete artefact belonging to another owner.");
        entity.Status = ArtefactStatus.Disabled;
        _db.Artefacts.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
