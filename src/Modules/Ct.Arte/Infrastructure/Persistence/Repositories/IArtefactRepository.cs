using Ct.Arte.Domain.Entities;

namespace Ct.Arte.Infrastructure.Persistence.Repositories;

public interface IArtefactRepository
{
    Task<Artefact?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Artefact?> GetByIdTrackedAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Artefact>> ListByOwnerAsync(string ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Artefact>> SearchAsync(string ownerId, string? search, ArtefactStatus? status, int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<Artefact>> GetByTagsAsync(string ownerId, IEnumerable<string> tags, CancellationToken ct = default);
    Task<IReadOnlyList<Artefact>> GetByMimeTypeAsync(string ownerId, string mimeType, CancellationToken ct = default);
    Task<Artefact?> GetByFilenameAsync(string ownerId, string fileName, CancellationToken ct = default);
    Task<bool> FilenameExistsAsync(string ownerId, string fileName, CancellationToken ct = default);
    Task AddAsync(Artefact artefact, CancellationToken ct = default);
    Task UpdateAsync(Artefact artefact, CancellationToken ct = default);
    Task SoftDeleteAsync(string id, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
