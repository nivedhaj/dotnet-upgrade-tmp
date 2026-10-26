using System.Text.Json;
using Ct.Staff.Application;
using Ct.Staff.Domain;
using Ct.Staff.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Staff.Tests;

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

    private static StaffDbContext CreateDbCore()
    {
        var options = new DbContextOptionsBuilder<StaffDbContext>()
            .UseNpgsql(GetConnectionString(), npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsql.CommandTimeout(30);
            })
            .EnableSensitiveDataLogging()
            .Options;
        return new StaffDbContext(options);
    }

    protected static StaffDbContext CreateDb()
    {
        EnsureSchema();
        return CreateDbCore();
    }

    protected static FixedStaffUserContext CreateUser(string? ownerId = null, string? userId = null, string? instId = null)
        => new(ownerId ?? Guid.NewGuid().ToString("D"), userId ?? "staff-test", instId);

    protected static StaffService CreateService(StaffDbContext db, IStaffUserContext user)
        => new(db, user);

    protected static SalaryService CreateSalaryService(StaffDbContext db, IStaffUserContext user)
        => new(db, user);

    protected static StaffReportService CreateReports(StaffService staff)
        => new(staff);

    protected static async Task CleanupAsync(StaffDbContext db, string ownerId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ownerId)) return;
        await db.Staff.Where(s => s.OwnerId == ownerId).ExecuteDeleteAsync(ct);
        await db.SalaryStructures.Where(s => s.OwnerId == ownerId).ExecuteDeleteAsync(ct);
    }

    protected static Guid IdOf(object mapped)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(mapped));
        return doc.RootElement.GetProperty("Id").GetGuid();
    }

    protected static string? PropString(object mapped, string name)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(mapped));
        return doc.RootElement.TryGetProperty(name, out var p) ? p.ToString() : null;
    }

    protected static CreateStaffRequest SampleStaff(
        string name = "Teacher A",
        string? email = "teacher.a@example.com",
        string? mobile = "9876543210",
        string? designation = "Teacher",
        string staffType = "Teaching",
        string? staffCode = null,
        List<string>? instituteIds = null,
        List<string>? subjects = null)
        => new()
        {
            Name = name,
            PrimaryEmail = email,
            PrimaryMobile = mobile,
            Designation = designation,
            StaffType = staffType,
            StaffCode = staffCode ?? $"S-{Guid.NewGuid().ToString("N")[..6]}",
            Gender = "Female",
            InstituteIds = instituteIds ?? [],
            Subjects = subjects ?? ["Math"]
        };
}

public sealed class FixedStaffUserContext(string ownerId, string? userId = "test", string? instId = null) : IStaffUserContext
{
    public string OwnerId { get; } = ownerId;
    public string? UserId { get; } = userId;
    public string? InstId { get; } = instId;
}
