using Ct.Arte.Application.Dtos;
using Ct.Arte.Application.Services;
using Ct.Arte.Domain.Entities;
using Ct.Arte.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ct.Arte.Tests;

/// <summary>
/// Covers previously missing IArtefactService methods:
/// attributes, video links, public URLs, description, reports,
/// template/model/url helpers, file names, widgets, ETL run, CreateTag.
/// </summary>
[TestClass]
public class ArtefactServiceExtendedTests : TestBase
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

    private void SetupTracked(Artefact entity)
    {
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupGetById(Artefact? entity, string id)
    {
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }

    // ---------- UpdateDescription ----------

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpdateDescriptionAsync_sets_description()
    {
        var entity = MakeArtefact();
        SetupTracked(entity);

        var ok = await _svc.UpdateDescriptionAsync(entity.Id, "new description");
        ok.Should().BeTrue();
        entity.Description.Should().Be("new description");
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpdateDescriptionAsync_returns_false_for_wrong_owner()
    {
        var entity = MakeArtefact(ownerId: "other");
        _repo.Setup(r => r.GetByIdTrackedAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var ok = await _svc.UpdateDescriptionAsync(entity.Id, "x");
        ok.Should().BeFalse();
    }

    // ---------- AddAttribute ----------

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task AddAttributeAsync_creates_block_and_key()
    {
        var entity = MakeArtefact();
        SetupTracked(entity);

        var ok = await _svc.AddAttributeAsync(entity.Id, "grade", "A+", itemId: 1);
        ok.Should().BeTrue();
        entity.DataAttributes.Should().ContainSingle(b => b.ItemId == 1);
        entity.DataAttributes[0].Attributes.Should().ContainSingle(a => a.Key == "grade" && (string?)a.Value == "A+");
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task AddAttributeAsync_replaces_existing_key_case_insensitive()
    {
        var entity = MakeArtefact();
        entity.DataAttributes.Add(new DataAttribute
        {
            ItemId = 1,
            Attributes = [new KeyValueAttr { Key = "Grade", Value = "B" }]
        });
        SetupTracked(entity);

        await _svc.AddAttributeAsync(entity.Id, "grade", "A", itemId: 1);

        entity.DataAttributes[0].Attributes.Should().ContainSingle();
        entity.DataAttributes[0].Attributes[0].Value.Should().Be("A");
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task AddAttributeAsync_returns_false_when_missing()
    {
        _repo.Setup(r => r.GetByIdTrackedAsync("nope", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Artefact?)null);

        var ok = await _svc.AddAttributeAsync("nope", "k", "v", 1);
        ok.Should().BeFalse();
    }

    // ---------- AddVideoLink ----------

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task AddVideoLinkAsync_parses_type_and_appends()
    {
        var entity = MakeArtefact();
        SetupTracked(entity);

        var ok = await _svc.AddVideoLinkAsync(entity.Id, "Stream", "https://video.example/1", itemId: 2);
        ok.Should().BeTrue();
        entity.VideoLinks.Should().ContainSingle(v =>
            v.Type == VideoType.Stream && v.Url == "https://video.example/1" && v.ItemId == 2);
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task AddVideoLinkAsync_defaults_unknown_type_to_zero()
    {
        var entity = MakeArtefact();
        SetupTracked(entity);

        await _svc.AddVideoLinkAsync(entity.Id, "NotARealType", "https://x", 1);
        entity.VideoLinks.Should().ContainSingle(v => v.Type == default);
    }

    // ---------- Public URLs ----------

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task UpdatePublicUrlAsync_adds_url_once()
    {
        var entity = MakeArtefact();
        SetupTracked(entity);

        await _svc.UpdatePublicUrlAsync(entity.Id, "https://cdn/a");
        await _svc.UpdatePublicUrlAsync(entity.Id, "https://cdn/a"); // duplicate

        entity.PublicUrls.Should().ContainSingle().Which.Should().Be("https://cdn/a");
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task RemovePublicUrlAsync_removes_matching()
    {
        var entity = MakeArtefact();
        entity.PublicUrls = ["https://cdn/a", "https://cdn/b"];
        SetupTracked(entity);

        await _svc.RemovePublicUrlAsync(entity.Id, "https://cdn/a");
        entity.PublicUrls.Should().BeEquivalentTo(["https://cdn/b"]);
    }

    [TestMethod]
    [TestCategory("Cmd")]
    public async Task ResetPublicUrlAsync_clears_all()
    {
        var entity = MakeArtefact();
        entity.PublicUrls = ["https://cdn/a", "https://cdn/b"];
        SetupTracked(entity);

        await _svc.ResetPublicUrlAsync(entity.Id);
        entity.PublicUrls.Should().BeEmpty();
    }

    // ---------- Reports / Template / Model / Url / FileNames ----------

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetReportsAsync_filters_report_tag()
    {
        var list = new List<Artefact> { MakeArtefact(tags: "report") };
        _repo.Setup(r => r.GetByTagsAsync(DefaultOwnerId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _svc.GetReportsAsync();
        result.Should().ContainSingle(a => a.Tags.Contains("report"));
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetTemplateAsync_returns_payload_or_null()
    {
        var entity = MakeArtefact(title: "Tmpl");
        entity.Template = "<html/>";
        SetupGetById(entity, entity.Id);
        SetupGetById(null, "missing");

        dynamic? found = await _svc.GetTemplateAsync(entity.Id);
        found.Should().NotBeNull();
        ((string)found!.Id).Should().Be(entity.Id);
        ((string)found.Template).Should().Be("<html/>");

        var missing = await _svc.GetTemplateAsync("missing");
        missing.Should().BeNull();
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetModelAsync_returns_payload_or_null()
    {
        var entity = MakeArtefact(title: "Model");
        entity.Model = "{ \"x\": 1 }";
        SetupGetById(entity, entity.Id);

        dynamic? found = await _svc.GetModelAsync(entity.Id);
        found.Should().NotBeNull();
        ((string)found!.Model).Should().Be("{ \"x\": 1 }");
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetUrlAsync_returns_urls()
    {
        var entity = MakeArtefact();
        entity.Url = "https://private/file";
        entity.PublicUrls = ["https://public/file"];
        SetupGetById(entity, entity.Id);

        dynamic? found = await _svc.GetUrlAsync(entity.Id);
        found.Should().NotBeNull();
        ((string)found!.Url).Should().Be("https://private/file");
    }

    [TestMethod]
    [TestCategory("Qry")]
    public async Task GetFileNamesAsync_projects_list()
    {
        var list = new List<Artefact>
        {
            MakeArtefact(fileName: "a.csv", title: "A", mimeType: "text/csv")
        };
        _repo.Setup(r => r.ListByOwnerAsync(DefaultOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        dynamic result = await _svc.GetFileNamesAsync();
        ((IEnumerable<object>)result).Should().NotBeEmpty();
    }

    // ---------- Widgets (service-level) ----------

    [TestMethod]
    [TestCategory("Widget")]
    public async Task WidgetBandwidthAsync_sums_file_sizes()
    {
        var list = new List<Artefact>
        {
            MakeArtefact(fileSize: 100),
            MakeArtefact(fileSize: 250)
        };
        _repo.Setup(r => r.ListByOwnerAsync(DefaultOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        dynamic result = await _svc.WidgetBandwidthAsync();
        ((double)result.totalBytes).Should().Be(350);
        ((int)result.count).Should().Be(2);
    }

    [TestMethod]
    [TestCategory("Widget")]
    public async Task WidgetAuthorCountAsync_groups_by_created_by()
    {
        var a = MakeArtefact();
        a.CreatedBy = "alice";
        var b = MakeArtefact();
        b.CreatedBy = "alice";
        var c = MakeArtefact();
        c.CreatedBy = "bob";
        _repo.Setup(r => r.ListByOwnerAsync(DefaultOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Artefact> { a, b, c });

        var result = await _svc.WidgetAuthorCountAsync();
        result.Should().NotBeNull();
    }

    [TestMethod]
    [TestCategory("Widget")]
    public async Task WidgetFileCountAsync_counts_named_files()
    {
        var withFile = MakeArtefact(fileName: "x.pdf");
        var noFile = MakeArtefact();
        noFile.FileName = null;
        _repo.Setup(r => r.ListByOwnerAsync(DefaultOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Artefact> { withFile, noFile });

        dynamic result = await _svc.WidgetFileCountAsync();
        ((int)result.files).Should().Be(1);
        ((int)result.total).Should().Be(2);
    }

    // ---------- CreateTag / ETL run ----------

    [TestMethod]
    [TestCategory("Stub")]
    public async Task CreateTagAsync_returns_note_payload()
    {
        dynamic result = await _svc.CreateTagAsync("github");
        ((string)result.name).Should().Be("github");
    }

    [TestMethod]
    [TestCategory("Stub")]
    public async Task RunEtlStubAsync_returns_queued()
    {
        dynamic result = await _svc.RunEtlStubAsync("arte-1");
        ((string)result.status).Should().Be("queued");
        ((string)result.artefactId).Should().Be("arte-1");
    }

    [TestMethod]
    [TestCategory("Stub")]
    public async Task RunEtlStubAsync_allows_null_id()
    {
        dynamic result = await _svc.RunEtlStubAsync(null);
        ((string)result.status).Should().Be("queued");
    }
}
