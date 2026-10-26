using Ct.Core.Infrastructure;
using Ct.Modules.Admissions.Application.Commands;
using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Application.Queries;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

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

    private static CtDbContext CreateDbCore()
    {
        var options = new DbContextOptionsBuilder<CtDbContext>()
            .UseNpgsql(GetConnectionString(), npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsql.CommandTimeout(30);
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "ct");
            })
            .EnableSensitiveDataLogging()
            .Options;
        return new CtDbContext(options);
    }

    protected static CtDbContext CreateDb()
    {
        EnsureSchema();
        return CreateDbCore();
    }

    protected static string NewInstId() => $"inst-{Guid.NewGuid():N}"[..20];

    protected static IStudentCommandService CreateCmds(CtDbContext db) => new StudentCommandService(db);
    protected static IStudentQueryService CreateQry(CtDbContext db) => new StudentQueryService(db);

    protected static CreateStudentRequest SampleStudent(
        string name = "Test Student",
        string? instId = null,
        string? courseId = "C1",
        string? courseName = "Class I",
        string? section = "A",
        string? gender = "Male",
        string? mobile = "9876543210",
        string? email = "stu@example.com",
        string? studentId = null)
        => new()
        {
            Name = name,
            Gender = gender,
            Mobile = mobile,
            Email = email,
            Dob = new DateOnly(2015, 5, 15),
            Doa = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-2)),
            InstId = instId ?? NewInstId(),
            CourseId = courseId,
            CourseName = courseName,
            SecName = section,
            TermName = "2026-27",
            StudentId = studentId,
            AadharNo = "1234-5678-9012",
            AdmissionNumber = "ADM-001",
            ApplicationNumber = "APP-001"
        };

    /// <summary>Delete students (and cascade relations) created in a test by id.</summary>
    protected static async Task CleanupAsync(CtDbContext db, IEnumerable<Guid> studentIds, CancellationToken ct = default)
    {
        var ids = studentIds.Distinct().ToList();
        if (ids.Count == 0) return;

        await db.StudentEnrollments.Where(e => ids.Contains(e.StudentId)).ExecuteDeleteAsync(ct);
        await db.StudentRelations.Where(r => ids.Contains(r.StudentId)).ExecuteDeleteAsync(ct);
        await db.StudentDomiciles.Where(d => ids.Contains(d.StudentId)).ExecuteDeleteAsync(ct);

        var personIds = await db.Students.Where(s => ids.Contains(s.Id)).Select(s => s.PersonId).ToListAsync(ct);
        await db.Students.Where(s => ids.Contains(s.Id)).ExecuteDeleteAsync(ct);

        if (personIds.Count > 0)
        {
            // contacts / addresses / tags cascade via person config if set; otherwise explicit
            await db.Persons.Where(p => personIds.Contains(p.Id)).ExecuteDeleteAsync(ct);
        }
    }
}
