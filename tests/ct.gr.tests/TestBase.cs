using Ct.Gr.Application;
using Ct.Gr.Domain;
using Ct.Gr.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Gr.Tests;

public abstract class TestBase
{
    public static IConfiguration Config { get; private set; } = null!;
    private static bool _schemaEnsured;
    private static readonly object SchemaLock = new();

    public const string DefaultUserId = "gr-test-user";

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

    private static GrDbContext CreateDbCore()
    {
        var options = new DbContextOptionsBuilder<GrDbContext>()
            .UseNpgsql(GetConnectionString(), npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsql.CommandTimeout(30);
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "gr");
            })
            .EnableSensitiveDataLogging()
            .Options;
        return new GrDbContext(options);
    }

    protected static GrDbContext CreateDb()
    {
        EnsureSchema();
        return CreateDbCore();
    }

    protected static FixedGrUserContext CreateUser(string? ownerId = null, string? userId = null)
        => new(ownerId ?? Guid.NewGuid().ToString("D"), userId ?? DefaultUserId);

    protected static ReceiptService CreateService(GrDbContext db, IGrUserContext user)
        => new(
            db,
            user,
            new ReceiptPrintService(),
            new ConfigGrPolicy(Config),
            new LoggingGrEventPublisher(NullLogger<LoggingGrEventPublisher>.Instance));

    protected static async Task CleanupAsync(GrDbContext db, string ownerId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ownerId)) return;
        await db.Receipts.Where(r => r.OwnerId == ownerId).ExecuteDeleteAsync(ct);
    }

    protected static CreateReceiptRequest SampleReceipt(
        string customerName = "Parent A",
        decimal amount = 1500m,
        string particular = "Tuition",
        string paymentMode = "Cash",
        string receiptType = "Regular",
        string? studyingIn = "I",
        string? section = "A")
        => new()
        {
            Date = DateTime.UtcNow.Date,
            PaymentMode = paymentMode,
            ReceiptType = receiptType,
            ReceivedBy = DefaultUserId,
            Include80G = receiptType.Equals("Donation", StringComparison.OrdinalIgnoreCase),
            Customer = new CustomerDto
            {
                Name = customerName,
                Address = "Addr 1",
                Contact = "9876543210",
                PAN = "ABCDE1234F",
                StudyingIn = studyingIn,
                Section = section,
                CustomerType = studyingIn is null ? "Regular" : "Student"
            },
            OrderItems =
            [
                new OrderItemDto
                {
                    Particular = particular,
                    Amount = amount,
                    Extras = new Dictionary<string, string> { ["note"] = "test" }
                }
            ]
        };

    protected static ReportFilterRequest DefaultFilter() => new()
    {
        From = DateTime.UtcNow.Date.AddMonths(-1),
        To = DateTime.UtcNow.Date.AddDays(1),
        Page = 1,
        PageSize = 50
    };
}

public sealed class FixedGrUserContext(string ownerId, string userId = "test") : IGrUserContext
{
    public string OwnerId { get; } = ownerId;
    public string UserId { get; } = userId;
}
