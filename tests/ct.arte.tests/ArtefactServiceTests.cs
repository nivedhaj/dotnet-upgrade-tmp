using Ct.Arte.Application.Dtos;
using Ct.Arte.Application.Services;
using Ct.Arte.Domain.Entities;
using Ct.Arte.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ct.Arte.Tests;

[TestClass]
public class ArtefactServiceTests : TestBase
{
    private Mock<IArtefactRepository> _repo = null!;
    private ICurrentUser _user = null!;
    private ArtefactService _svc = null!;

    [TestInitialize]
    public void Init()
    {
        _repo = CreateRepoMock();
        _user = CreateCurrentUser();
        _svc = new ArtefactService(_repo.Object, _user);
    }

    // ---------- Get / Search / List ----------

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetAsync_returns_dto_when_found()
    {
        var entity = MakeArtefact(title: "Report Q1");
        _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var dto = await _svc.GetAsync(entity.Id);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(entity.Id);
        dto.Title.Should().Be("Report Q1");
        dto.OwnerId.Should().Be(DefaultOwnerId);
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetAsync_returns_null_when_missing()
    {
        _repo.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Artefact?)null);

        var dto = await _svc.GetAsync("missing");
        dto.Should().BeNull();
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task SearchAsync_delegates_to_repo_with_owner()
    {
        var list = new List<Artefact>
        {
            MakeArtefact(title: "A"),
            MakeArtefact(title: "B")
        };
        _repo.Setup(r => r.SearchAsync(DefaultOwnerId, "csv", null, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _svc.SearchAsync(new ArtefactSearchRequest { Search = "csv", Skip = 0, Take = 50 });

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("A");
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetAllAsync_returns_owner_list()
    {
        var list = new List<Artefact> { MakeArtefact() };
        _repo.Setup(r => r.ListByOwnerAsync(DefaultOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _svc.GetAllAsync();
        result.Should().HaveCount(1);
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetByMimeTypeAsync_filters_by_mime()
    {
        var list = new List<Artefact>
        {
            MakeArtefact(mimeType: "text/csv", title: "students.csv")
        };
        _repo.Setup(r => r.GetByMimeTypeAsync(DefaultOwnerId, "text/csv", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _svc.GetByMimeTypeAsync("text/csv");
        result.Should().ContainSingle(a => a.MimeType == "text/csv");
    }

    // ---------- Create / Upsert / Update ----------

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task CreateAsync_adds_entity_and_saves()
    {
        Artefact? added = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Artefact>(), It.IsAny<CancellationToken>()))
            .Callback<Artefact, CancellationToken>((a, _) => added = a)
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var req = new CreateArtefactRequest
        {
            Title = "New Doc",
            FileName = "doc.pdf",
            MimeType = "application/pdf",
            FileSize = 4096,
            Tags = ["report"]
        };

        var dto = await _svc.CreateAsync(req);

        added.Should().NotBeNull();
        added!.Title.Should().Be("New Doc");
        added.OwnerId.Should().Be(DefaultOwnerId);
        added.Status.Should().Be(ArtefactStatus.Active);
        dto.Title.Should().Be("New Doc");
        dto.Tags.Should().Contain("report");
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpsertAsync_creates_when_filename_missing()
    {
        _repo.Setup(r => r.GetByFilenameAsync(DefaultOwnerId, "new.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Artefact?)null);
        _repo.Setup(r => r.AddAsync(It.IsAny<Artefact>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = await _svc.UpsertAsync(new CreateArtefactRequest
        {
            Title = "Upsert New",
            FileName = "new.txt"
        });

        dto.Title.Should().Be("Upsert New");
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpsertAsync_updates_when_filename_exists()
    {
        var existing = MakeArtefact(fileName: "existing.txt", title: "Old");
        _repo.Setup(r => r.GetByFilenameAsync(DefaultOwnerId, "existing.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _repo.Setup(r => r.GetByIdTrackedAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = await _svc.UpsertAsync(new CreateArtefactRequest
        {
            Title = "Updated Title",
            FileName = "existing.txt",
            Description = "new desc"
        });

        existing.Title.Should().Be("Updated Title");
        existing.Description.Should().Be("new desc");
        dto.Title.Should().Be("Updated Title");
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpdateAsync_returns_null_for_wrong_owner()
    {
        var entity = MakeArtefact(ownerId: "other-owner");
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _svc.UpdateAsync(entity.Id, new UpdateArtefactRequest { Title = "Hacked" });
        result.Should().BeNull();
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpdateAsync_applies_fields_for_own_artefact()
    {
        var entity = MakeArtefact(title: "Before");
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = await _svc.UpdateAsync(entity.Id, new UpdateArtefactRequest
        {
            Title = "After",
            Description = "Updated desc",
            Tags = ["v2"]
        });

        dto.Should().NotBeNull();
        entity.Title.Should().Be("After");
        entity.Description.Should().Be("Updated desc");
        entity.Tags.Should().Contain("v2");
    }

    // ---------- Enable / Disable / Publish ----------

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task EnableAsync_sets_Active()
    {
        var entity = MakeArtefact(status: ArtefactStatus.Disabled);
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var ok = await _svc.EnableAsync(entity.Id);
        ok.Should().BeTrue();
        entity.Status.Should().Be(ArtefactStatus.Active);
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task DisableAsync_sets_Disabled()
    {
        var entity = MakeArtefact();
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var ok = await _svc.DisableAsync(entity.Id);
        ok.Should().BeTrue();
        entity.Status.Should().Be(ArtefactStatus.Disabled);
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task PublishAsync_sets_Published_and_timestamp()
    {
        var entity = MakeArtefact();
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var ok = await _svc.PublishAsync(entity.Id, toPublic: false);
        ok.Should().BeTrue();
        entity.Status.Should().Be(ArtefactStatus.Published);
        entity.PublishedOn.Should().NotBeNull();
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task PublishAsync_toPublic_sets_PublishedToPublic()
    {
        var entity = MakeArtefact();
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var ok = await _svc.PublishAsync(entity.Id, toPublic: true);
        ok.Should().BeTrue();
        entity.Status.Should().Be(ArtefactStatus.PublishedToPublic);
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UnpublishAsync_reverts_to_Active()
    {
        var entity = MakeArtefact(status: ArtefactStatus.Published);
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var ok = await _svc.UnpublishAsync(entity.Id, publicOnly: false);
        ok.Should().BeTrue();
        entity.Status.Should().Be(ArtefactStatus.Active);
    }

    // ---------- Tags / Comments / Attributes ----------

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpdateTagsAsync_replaces_tags()
    {
        var entity = MakeArtefact(tags: "old");
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var ok = await _svc.UpdateTagsAsync(entity.Id, ["github", "spec"]);
        ok.Should().BeTrue();
        entity.Tags.Should().BeEquivalentTo(["github", "spec"]);
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task AddCommentAsync_appends_comment()
    {
        var entity = MakeArtefact();
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var ok = await _svc.AddCommentAsync(entity.Id, "Looks good", "<p>Looks good</p>");
        ok.Should().BeTrue();
        entity.Comments.Should().ContainSingle(c => c.Text == "Looks good" && c.By == DefaultUserId);
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpsertStudentDocumentAsync_adds_student_document_tag()
    {
        _repo.Setup(r => r.GetByFilenameAsync(DefaultOwnerId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Artefact?)null);
        Artefact? added = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Artefact>(), It.IsAny<CancellationToken>()))
            .Callback<Artefact, CancellationToken>((a, _) => added = a)
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _svc.UpsertStudentDocumentAsync(new CreateArtefactRequest
        {
            Title = "Aadhar",
            FileName = "aadhar.pdf"
        });

        added.Should().NotBeNull();
        added!.Tags.Should().Contain("student-document");
    }

    // ---------- Soft delete ----------

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task SoftDeleteAsync_calls_repo()
    {
        _repo.Setup(r => r.SoftDeleteAsync("id-1", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _svc.SoftDeleteAsync("id-1");

        _repo.Verify(r => r.SoftDeleteAsync("id-1", It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- Widgets & stubs ----------

    [TestMethod]
    [TestCategory("Widget")]
    public async Task WidgetCountAsync_aggregates_by_status()
    {
        var list = new List<Artefact>
        {
            MakeArtefact(status: ArtefactStatus.Active),
            MakeArtefact(status: ArtefactStatus.Active),
            MakeArtefact(status: ArtefactStatus.Published)
        };
        _repo.Setup(r => r.ListByOwnerAsync(DefaultOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _svc.WidgetCountAsync();
        result.Should().NotBeNull();
    }

    [TestMethod]
    [TestCategory("Widget")]
    public async Task WidgetSizeBucketsAsync_groups_by_size()
    {
        var list = new List<Artefact>
        {
            MakeArtefact(fileSize: 500),
            MakeArtefact(fileSize: 2_000_000),
            MakeArtefact(fileSize: 15_000_000)
        };
        _repo.Setup(r => r.ListByOwnerAsync(DefaultOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        dynamic buckets = await _svc.WidgetSizeBucketsAsync();
        ((int)buckets.small).Should().Be(1);
        ((int)buckets.medium).Should().Be(1);
        ((int)buckets.large).Should().Be(1);
    }

    [TestMethod]
    [TestCategory("Stub")]
    public async Task GenerateEtlStubAsync_returns_scaffold()
    {
        dynamic result = await _svc.GenerateEtlStubAsync("student");
        ((string)result.kind).Should().Be("student");
        ((string)result.status).Should().Be("generated");
    }

    [TestMethod]
    [TestCategory("Stub")]
    public async Task StudentAttendanceTemplateAsync_returns_csv_bytes()
    {
        var bytes = await _svc.StudentAttendanceTemplateAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().StartWith("StudentId,Name,Course");
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetTagsAsync_returns_distinct_sorted()
    {
        var list = new List<Artefact>
        {
            MakeArtefact(tags: "github", "spec"),
            MakeArtefact(tags: "Github", "report")
        };
        _repo.Setup(r => r.ListByOwnerAsync(DefaultOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var tags = await _svc.GetTagsAsync();
        tags.Should().BeEquivalentTo(["github", "report", "spec"]);
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetDataTemplatesAsync_filters_by_mode()
    {
        var live = MakeArtefact(status: ArtefactStatus.Published, tags: "data-template");
        var working = MakeArtefact(status: ArtefactStatus.Active, tags: "template");
        _repo.Setup(r => r.GetByTagsAsync(DefaultOwnerId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Artefact> { live, working });

        var liveOnly = await _svc.GetDataTemplatesAsync("live");
        liveOnly.Should().ContainSingle(a => a.Status == ArtefactStatus.Published);

        var workingOnly = await _svc.GetDataTemplatesAsync("working");
        workingOnly.Should().ContainSingle(a => a.Status == ArtefactStatus.Active);
    }
}
