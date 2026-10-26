using Ct.Arte.Domain.Entities;
using Ct.Arte.Infrastructure.Persistence;
using Ct.Arte.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Ct.Arte.Tests;

/// <summary>
/// Shared fixtures for Ct.Arte unit / integration tests (replaces Raven TestBase).
/// </summary>
public abstract class TestBase
{
    public const string DefaultOwnerId = "test-owner-0001";
    public const string DefaultUserId = "test-user-0001";

    protected static ICurrentUser CreateCurrentUser(
        string ownerId = DefaultOwnerId,
        string userId = DefaultUserId,
        bool isRootAdmin = false)
        => new CurrentUser
        {
            OwnerId = ownerId,
            UserId = userId,
            Email = $"{userId}@test.local",
            IsRootAdmin = isRootAdmin
        };

    protected static Artefact MakeArtefact(
        string? id = null,
        string ownerId = DefaultOwnerId,
        string title = "Sample Artefact",
        ArtefactStatus status = ArtefactStatus.Active,
        string? fileName = null,
        string? mimeType = null,
        double fileSize = 1024,
        params string[] tags)
    {
        var a = new Artefact
        {
            Title = title,
            Description = "Test description",
            FileName = fileName ?? $"{Guid.NewGuid():N}.txt",
            MimeType = mimeType ?? "text/plain",
            FileSize = fileSize,
            Status = status,
            Tags = tags.ToList(),
            MetaData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["source"] = "unit-test"
            }
        };
        a.EnsureId(id);
        a.SetOwner(ownerId);
        a.CreatedOn = DateTimeOffset.UtcNow;
        a.CreatedBy = DefaultUserId;
        return a;
    }

    /// <summary>
    /// Builds an in-memory ArtefactDbContext for repository-level tests.
    /// </summary>
    protected static ArtefactDbContext CreateInMemoryDb(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ArtefactDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString("N"))
            .Options;
        return new ArtefactDbContext(options);
    }

    protected static Mock<IArtefactRepository> CreateRepoMock() => new(MockBehavior.Strict);
}
