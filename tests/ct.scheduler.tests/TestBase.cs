using System.Text.Json;
using Ct.Scheduler.Application;
using Ct.Scheduler.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Scheduler.Tests;

public abstract class TestBase
{
    public static IConfiguration Config { get; private set; } = null!;
    private static bool _schemaEnsured;
    private static readonly object SchemaLock = new();

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void ClassInit(TestContext _)
    {
        Configure();
        EnsureSchema();
    }

    public static void Configure()
    {
        if (Config is not null) return;
        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets<TestBase>(optional: true)
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

    private static SchedulerDbContext CreateDbCore()
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseNpgsql(GetConnectionString(), npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsql.CommandTimeout(30);
            })
            .EnableSensitiveDataLogging()
            .Options;
        return new SchedulerDbContext(options);
    }

    protected static SchedulerDbContext CreateDb()
    {
        EnsureSchema();
        return CreateDbCore();
    }

    protected static FixedSchedulerUserContext CreateUser(string? ownerId = null, string? userId = null, string? instId = null)
        => new(ownerId ?? Guid.NewGuid().ToString("D"), userId ?? "sched-test", instId);

    protected static CalendarService CreateCalendar(SchedulerDbContext db, ISchedulerUserContext user)
        => new(db, user);

    protected static SchedulerService CreateScheduler(CalendarService calendar)
        => new(calendar);

    protected static SchedulerReportService CreateReports(CalendarService calendar)
        => new(calendar);

    protected static CalendarCmd CreateCmd(CalendarService calendar) => new(calendar);
    protected static CalendarQry CreateQry(CalendarService calendar) => new(calendar);
    protected static SchedulerReportCmd CreateReportCmd(CalendarService calendar, SchedulerReportService reports)
        => new(calendar, reports);

    protected static async Task CleanupAsync(SchedulerDbContext db, string ownerId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ownerId)) return;
        await db.Events.Where(e => e.OwnerId == ownerId).ExecuteDeleteAsync(ct);
        await db.Rules.Where(r => r.OwnerId == ownerId).ExecuteDeleteAsync(ct);
    }

    protected static Guid IdOf(object mapped)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(mapped));
        return doc.RootElement.GetProperty("Id").GetGuid();
    }

    protected static int IntProp(object mapped, string name)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(mapped));
        return doc.RootElement.GetProperty(name).GetInt32();
    }

    protected static DateTime DateProp(object mapped, string name)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(mapped));
        return doc.RootElement.GetProperty(name).GetDateTime();
    }
}

public sealed class FixedSchedulerUserContext(string ownerId, string? userId = "test", string? instId = null) : ISchedulerUserContext
{
    public string OwnerId { get; } = ownerId;
    public string? UserId { get; } = userId;
    public string? InstId { get; } = instId;
}
