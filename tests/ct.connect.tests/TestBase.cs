using Ct.Connect.Application;
using Ct.Connect.Infrastructure.Persistence;
using Ct.Connect.Infrastructure.Repositories;
using Ct.Connect.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Connect.Tests;

/// <summary>
/// Shared fixtures for Ct.Connect tests.
/// Uses EF Core + Postgres (Npgsql) against ConnectionStrings:Postgres.
/// Each test gets an isolated institute OwnerId so rows do not collide;
/// call CleanupAsync to delete rows owned by that test inst.
/// </summary>
public abstract class TestBase
{
    public static IConfiguration Config { get; private set; } = null!;
    private static bool _schemaEnsured;
    private static readonly object SchemaLock = new();

    // Sample institute ids (kept from original suite for familiarity)
    public const string IdBtlCol1718 = "0c250000-50bf-11e7-89e2-1cbdb9e7fd04";
    public const string IdBtlCol2021 = "0c250000-2021-11e7-89e2-1cbdb9e7fd04";
    public const string IdVvspu1920 = "1a6a6000-2021-45ec-9500-f37f54f82d5f";
    public const string IdSppu1920 = "1a6a5000-1920-45ec-9500-f37f54f82d5f";
    public const string IdSppu2021 = "1a6a5000-2021-45ec-9500-f37f54f82d5f";

    public const string DefaultUserId = "9a5fab7d-ee6f-40a9-8e09-ba8cad8f6432";

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void ClassInit(TestContext _)
    {
        Configure();
        EnsureSchema();
    }

    public static void Configure()
    {
        if (Config is not null) return;

        var built = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets<TestBase>(optional: true)
            .Build();

        // Overlay defaults for Connect settings if missing
        var defaults = new Dictionary<string, string?>();
        if (string.IsNullOrWhiteSpace(built["Connect:Sms:SenderId"]))
            defaults["Connect:Sms:SenderId"] = "TESTSEND";
        if (string.IsNullOrWhiteSpace(built["Connect:Email:From"]))
            defaults["Connect:Email:From"] = "noreply@test.local";
        if (string.IsNullOrWhiteSpace(built["Connect:RabbitMq:Enabled"]))
            defaults["Connect:RabbitMq:Enabled"] = "false";

        Config = defaults.Count == 0
            ? built
            : new ConfigurationBuilder()
                .AddConfiguration(built)
                .AddInMemoryCollection(defaults)
                .Build();
    }

    public static string GetConnectionString()
    {
        Configure();
        return
            Environment.GetEnvironmentVariable("CT_TEST_POSTGRES")
            ?? Config.GetConnectionString("Postgres")
            ?? Config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=ct_modular_test;Username=ct;Password=ct_dev_password";
    }

    /// <summary>
    /// Ensures connect tables exist via EnsureCreated.
    /// Prefer migrations in shared environments; for a dedicated test DB this is self-contained.
    /// </summary>
    public static void EnsureSchema()
    {
        lock (SchemaLock)
        {
            if (_schemaEnsured) return;
            using var db = CreateDbCore();
            db.Database.EnsureCreated();
            _schemaEnsured = true;
        }
    }

    private static ConnectDbContext CreateDbCore()
    {
        var options = new DbContextOptionsBuilder<ConnectDbContext>()
            .UseNpgsql(GetConnectionString(), npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsql.CommandTimeout(30);
            })
            .EnableSensitiveDataLogging()
            .Options;
        return new ConnectDbContext(options);
    }

    /// <summary>Opens a Postgres-backed ConnectDbContext.</summary>
    protected static ConnectDbContext CreateDb()
    {
        EnsureSchema();
        return CreateDbCore();
    }

    /// <summary>
    /// Creates a user context. By default InstId is a fresh GUID for isolation.
    /// Pass <paramref name="instId"/> to use a fixed institute; set uniqueInst: false
    /// only when you intentionally share state.
    /// </summary>
    protected static TestUserContext CreateUser(
        string? instId = null,
        string? userId = null,
        bool boundByInst = true,
        bool uniqueInst = true)
    {
        var owner = instId
            ?? (uniqueInst ? Guid.NewGuid().ToString("D") : IdSppu1920);

        return new TestUserContext
        {
            SessionId = Guid.NewGuid().ToString("N"),
            UserId = userId ?? DefaultUserId,
            InstId = owner,
            OrgId = "",
            BoundByInst = boundByInst,
            BoundByOrg = false,
            BoundByUser = false,
            IsRootAdmin = false,
            Persist = true
        };
    }

    /// <summary>
    /// Deletes all connect rows owned by the given institute (test isolation).
    /// </summary>
    protected static async Task CleanupAsync(ConnectDbContext db, string ownerId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ownerId)) return;

        await db.Topics.Where(t => t.OwnerId == ownerId).ExecuteDeleteAsync(ct);
        await db.SmsMessages.Where(m => m.OwnerId == ownerId).ExecuteDeleteAsync(ct);
        await db.Sms.Where(s => s.OwnerId == ownerId).ExecuteDeleteAsync(ct);
        await db.Emails.Where(e => e.OwnerId == ownerId).ExecuteDeleteAsync(ct);
    }

    protected static FixedUserContextAccessor CreateAccessor(IUserContext user)
        => new(user);

    protected static IConnectEventPublisher CreateEvents()
        => new LoggingConnectEventPublisher(NullLogger<LoggingConnectEventPublisher>.Instance);

    protected static ILogger<T> Logger<T>() => NullLogger<T>.Instance;

    protected static TopicCmd CreateTopicCmd(ConnectDbContext db, IUserContext user)
        => new(db, CreateAccessor(user), CreateEvents());

    protected static ConnectCmd CreateConnectCmd(ConnectDbContext db, IUserContext user)
        => new(db, CreateAccessor(user), CreateEvents(), Config, Logger<ConnectCmd>());

    protected static ConnectQry CreateConnectQry(ConnectDbContext db, IUserContext user)
        => new(db, CreateAccessor(user), CreateEvents(), Config);

    protected static WorkflowService CreateWorkflowService(IUserContext user)
        => new(CreateAccessor(user), CreateEvents(), Logger<WorkflowService>());
}

/// <summary>Mutable test double for IUserContext.</summary>
public sealed class TestUserContext : IUserContext
{
    public string SessionId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string InstId { get; set; } = "";
    public string OrgId { get; set; } = "";
    public bool BoundByInst { get; set; } = true;
    public bool BoundByOrg { get; set; }
    public bool BoundByUser { get; set; }
    public bool IsRootAdmin { get; set; }
    public string? Token { get; set; }
    public bool Persist { get; set; } = true;
}

public sealed class FixedUserContextAccessor(IUserContext current) : IUserContextAccessor
{
    public IUserContext? Current { get; } = current;
}
