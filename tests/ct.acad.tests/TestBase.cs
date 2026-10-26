using Ct.Acad.Application;
using Ct.Acad.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Acad.Tests;

/// <summary>
/// Shared fixtures for Ct.Acad tests (replaces Raven TestBase).
/// Uses EF Core InMemory so services that take AcadDbContext can run without Postgres.
/// </summary>
public abstract class TestBase
{
    public const string DefaultOwnerId = "test-owner-acad-0001";
    public const string DefaultUserId = "test-user-acad-0001";

    protected static IAcadUserContext CreateUser(
        string ownerId = DefaultOwnerId,
        string userId = DefaultUserId)
        => new FixedAcadUser(ownerId, userId);

    protected static AcadDbContext CreateDb(string? name = null)
    {
        var options = new DbContextOptionsBuilder<AcadDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString("N"))
            .Options;
        return new AcadDbContext(options);
    }

    protected sealed class FixedAcadUser(string ownerId, string userId) : IAcadUserContext
    {
        public string OwnerId { get; } = ownerId;
        public string UserId { get; } = userId;
    }
}
