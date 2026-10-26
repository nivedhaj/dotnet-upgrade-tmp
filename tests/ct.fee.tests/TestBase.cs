using Ct.Fee.Application;
using Ct.Fee.Application.Integrations;
using Ct.Fee.Application.Policy;
using Ct.Fee.Domain;
using Ct.Fee.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

/// <summary>
/// Postgres-backed fixtures for Ct.Fee tests.
/// Covers the full surface of the legacy ct.fee.tests suite against modular FeeService / ReportService.
/// </summary>
public abstract class TestBase
{
    public static IConfiguration Config { get; private set; } = null!;
    private static bool _schemaEnsured;
    private static readonly object SchemaLock = new();

    public static readonly Guid SampleOwnerBtl = Guid.Parse("0c250000-50bf-11e7-89e2-1cbdb9e7fd04");
    public const string DefaultUserId = "fee-test-user";

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
            using var db = CreateDbCore(new FixedCurrentUserContext(Guid.Empty, "schema"));
            db.Database.EnsureCreated();
            _schemaEnsured = true;
        }
    }

    private static FeeDbContext CreateDbCore(ICurrentUserContext user)
    {
        var options = new DbContextOptionsBuilder<FeeDbContext>()
            .UseNpgsql(GetConnectionString(), npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsql.CommandTimeout(30);
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "fee");
            })
            .EnableSensitiveDataLogging()
            .Options;
        return new FeeDbContext(options, user);
    }

    protected static FeeDbContext CreateDb(ICurrentUserContext user)
    {
        EnsureSchema();
        return CreateDbCore(user);
    }

    protected static FixedCurrentUserContext CreateUser(Guid? ownerId = null, string? userId = null)
        => new(ownerId ?? Guid.NewGuid(), userId ?? DefaultUserId);

    protected static FeeService CreateFeeService(FeeDbContext db, ICurrentUserContext user, IFeePolicyEngine? policy = null)
    {
        policy ??= new DefaultFeePolicyEngine();
        return new FeeService(
            db,
            new FeeCalculationService(),
            user,
            policy,
            new NullStudentDirectory(),
            new NullStudentFeeReceivableClient(),
            new LoggingFeeEventPublisher(NullLogger<LoggingFeeEventPublisher>.Instance));
    }

    protected static ReportService CreateReportService(FeeDbContext db, ICurrentUserContext user, IFeePolicyEngine? policy = null)
        => new(db, user, new NullStudentDirectory(), policy ?? new DefaultFeePolicyEngine());

    protected static IFeeEventPublisher CreateEvents()
        => new LoggingFeeEventPublisher(NullLogger<LoggingFeeEventPublisher>.Instance);

    protected static async Task CleanupAsync(FeeDbContext db, Guid ownerId, CancellationToken ct = default)
    {
        if (ownerId == Guid.Empty) return;
        await db.SuspenseAccountTransactions.Where(x => x.OwnerId == ownerId).ExecuteDeleteAsync(ct);
        await db.Refunds.Where(x => x.OwnerId == ownerId).ExecuteDeleteAsync(ct);
        await db.FeeTransactions.Where(x => x.OwnerId == ownerId).ExecuteDeleteAsync(ct);
        await db.Fees.Where(x => x.OwnerId == ownerId).ExecuteDeleteAsync(ct);
    }

    protected static FeeDefinitionRequest SampleFeeRequest(
        string name = "Tuition Test",
        decimal amount = 10000m,
        string? studentId = null,
        string? courseId = null,
        DateOnly? due1 = null,
        DateOnly? due2 = null)
    {
        due1 ??= DateOnly.FromDateTime(DateTime.UtcNow.Date);
        due2 ??= DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(3));
        return new FeeDefinitionRequest(
            Name: name,
            DisplayText: name,
            Amount: amount,
            Tags: ["test"],
            CollectStudentWise: studentId is not null,
            SpansAcademicYears: false,
            Installments:
            [
                new FeeInstallmentDto(1, "Installment 1", amount / 2, due1),
                new FeeInstallmentDto(2, "Installment 2", amount / 2, due2)
            ],
            Fines: [new FeeFineDto(1, "Late fee", 100m, 15)],
            StudentList: studentId is null ? [] : [studentId],
            CourseList: courseId is null ? [] : [courseId]);
    }

    protected static ReportFilterRequest DefaultFilter(string? studentId = null, string? courseId = null) =>
        new(
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            courseId, studentId, null, null, null, null);

    /// <summary>Seed fee + partial payment for report/tx scenarios.</summary>
    protected static async Task<(FeeDefinitionDto Fee, FeeTransactionDto Tx)> SeedPaidFeeAsync(
        FeeService svc, string studentId = "stu-seed", decimal feeAmount = 10000m, decimal payAmount = 5000m)
    {
        var fee = await svc.CreateFeeAsync(SampleFeeRequest($"Seed {Guid.NewGuid():N}"[..20], feeAmount, studentId));
        var tx = await svc.CreateTransactionAsync(new FeeTransactionRequest(
            TxNo: $"TX-{Guid.NewGuid():N}"[..12],
            TxDate: DateTimeOffset.UtcNow,
            StudentId: studentId,
            PaymentMode: "Cash",
            IsOpeningBalanceAdjusted: false,
            RefNo: $"REF-{Guid.NewGuid():N}"[..12],
            ReceivedInAccount: "Cash",
            PaidBy: "Parent",
            InstallmentsPaid: [new TxLine(fee.Id, 1, payAmount)],
            FinesPaid: [],
            Discounts: [],
            Adjustments: []));
        return (fee, tx);
    }
}
