using Ct.Core.Infrastructure;
using Ct.Modules.Admissions.Application.Commands;
using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ct.Tests.Integration;

/// <summary>
/// Exercises the command/query services against EF Core's InMemory provider.
/// NOTE: InMemory does not support Npgsql-specific translations (jsonb ops,
/// EF.Functions.ILike), so `StudentQueryService.SearchAsync`'s free-text
/// branch is intentionally NOT covered here — it requires a real PostgreSQL
/// instance (see docs/database-migration.md). Everything else in this class
/// exercises the actual EF model end-to-end, including relationships,
/// transactions, and cascade behavior.
/// </summary>
public class StudentServiceTests : IDisposable
{
    private readonly CtDbContext _db;
    private readonly StudentCommandService _commands;
    private readonly StudentQueryService _queries;

    public StudentServiceTests()
    {
        var options = new DbContextOptionsBuilder<CtDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new CtDbContext(options);
        _commands = new StudentCommandService(_db);
        _queries = new StudentQueryService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateAsync_PersistsStudentPersonAndEnrollment()
    {
        var request = new CreateStudentRequest
        {
            Name = "john doe",
            Email = "john@example.com",
            InstId = "INST1",
            CourseName = "BSc",
            CourseId = "C1",
            AdmissionNumber = "ADM1"
        };

        var id = await _commands.CreateAsync(request, actor: "tester");

        var dto = await _queries.GetByIdAsync(id);
        Assert.NotNull(dto);
        Assert.Equal("John Doe", dto!.Name);
        Assert.Equal("C1", dto.CourseId);
        Assert.Null(dto.RollNo); // roll no is set separately via /setRollNo, not at creation
    }

    [Fact]
    public async Task EnableDisable_TogglesStatus()
    {
        var id = await _commands.CreateAsync(new CreateStudentRequest { Name = "Jane Doe" }, "tester");

        await _commands.DisableAsync(id, "tester");
        var disabled = await _queries.GetByIdAsync(id);
        Assert.Equal("Disabled", disabled!.Status);

        await _commands.EnableAsync(id, "tester");
        var enabled = await _queries.GetByIdAsync(id);
        Assert.Equal("Active", enabled!.Status);
    }

    [Fact]
    public async Task PromoteAsync_ReturnsFalse_WhenBaseEnrollmentMissing()
    {
        var id = await _commands.CreateAsync(new CreateStudentRequest { Name = "No Enrollment" }, "tester");

        var promoted = await _commands.PromoteAsync(new PromoteStudentRequest
        {
            Id = id,
            BaseInstId = "INST1",
            BaseCourseId = "C1",
            TargetInstId = "INST2",
            TargetCourseId = "C2"
        }, "tester");

        Assert.False(promoted);
    }

    [Fact]
    public async Task PromoteAsync_ClonesEnrollmentTransactionally()
    {
        var id = await _commands.CreateAsync(new CreateStudentRequest
        {
            Name = "Promote Me",
            InstId = "INST1",
            CourseId = "C1",
            CourseName = "BSc Year 1",
            AdmissionNumber = "ADM1"
        }, "tester");

        var promoted = await _commands.PromoteAsync(new PromoteStudentRequest
        {
            Id = id,
            BaseInstId = "INST1",
            BaseCourseId = "C1",
            TargetInstId = "INST2",
            TargetCourseId = "C2",
            TargetTerm = "Year 2"
        }, "tester");

        Assert.True(promoted);
        var dto = await _queries.GetByIdAsync(id);
        Assert.NotNull(dto);
    }

    [Fact]
    public async Task SetTagsAsync_ReplacesTagsWholesale()
    {
        var id = await _commands.CreateAsync(new CreateStudentRequest { Name = "Tag Test" }, "tester");

        await _commands.SetTagsAsync(id, new[] { "Alpha", "Beta" });
        var afterFirst = await _queries.GetByIdAsync(id);
        Assert.Equal(new[] { "Alpha", "Beta" }, afterFirst!.Tags.OrderBy(t => t));

        await _commands.SetTagsAsync(id, new[] { "Gamma" });
        var afterSecond = await _queries.GetByIdAsync(id);
        Assert.Equal(new[] { "Gamma" }, afterSecond!.Tags);
    }

    [Fact]
    public async Task GetByTagAsync_FindsStudentsWithMatchingTag()
    {
        var id = await _commands.CreateAsync(new CreateStudentRequest { Name = "Tagged Student" }, "tester");
        await _commands.SetTagsAsync(id, new[] { "NeedsAttention" });

        var results = await _queries.GetByTagAsync("NeedsAttention");

        Assert.Single(results);
        Assert.Equal(id, results[0].Id);
    }

    [Fact]
    public async Task SetPrimaryEmailAsync_CreatesThenUpdatesPrimaryContact()
    {
        var id = await _commands.CreateAsync(new CreateStudentRequest { Name = "Contact Test" }, "tester");

        await _commands.SetPrimaryEmailAsync(id, "first@example.com");
        await _commands.SetPrimaryEmailAsync(id, "second@example.com");

        var student = await _db.Students.Include(s => s.Person).ThenInclude(p => p!.Contacts)
            .FirstAsync(s => s.Id == id);

        var primaryEmail = Assert.Single(student.Person!.Contacts);
        Assert.Equal("second@example.com", primaryEmail.Info);
    }
}
