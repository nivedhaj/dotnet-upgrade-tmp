using Ct.Arte.Application.Dtos;
using Ct.Arte.Application.Services;
using Ct.Arte.Controllers;
using Ct.Arte.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ct.Arte.Tests;

/// <summary>
/// Covers ArtefactController routes not exercised by ArtefactControllerTests:
/// update/disable/unpublish, download/content, extract, student docs,
/// comments/attributes/video, public URLs, data templates, tags, ETL, removeFile.
/// </summary>
[TestClass]
public class ArtefactControllerExtendedTests : TestBase
{
    private Mock<IArtefactService> _svc = null!;
    private ArtefactController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _svc = new Mock<IArtefactService>(MockBehavior.Strict);
        _ctrl = new ArtefactController(_svc.Object);
    }

    private static ArtefactDto SampleDto(string id = "a1", string title = "Doc")
        => new(
            Id: id,
            OwnerId: DefaultOwnerId,
            Title: title,
            Description: "desc",
            Url: "https://cdn.example/doc.pdf",
            MimeType: "application/pdf",
            FileName: "doc.pdf",
            FileSize: 1024,
            Sha1: null,
            Status: ArtefactStatus.Active,
            Model: null,
            CreatedOn: DateTimeOffset.UtcNow,
            CreatedBy: DefaultUserId,
            ModifiedOn: null,
            Tags: ["report"],
            MetaData: new Dictionary<string, string>(),
            PublishedOn: null);

    // ---------- Update / Disable / Unpublish ----------

    [TestMethod]
    public async Task Update_returns_Ok_when_found()
    {
        var dto = SampleDto();
        _svc.Setup(s => s.UpdateAsync("a1", It.IsAny<UpdateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _ctrl.Update("a1", new UpdateArtefactRequest { Title = "Updated" }, default);
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(dto);
    }

    [TestMethod]
    public async Task Update_returns_NotFound_when_missing()
    {
        _svc.Setup(s => s.UpdateAsync("x", It.IsAny<UpdateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtefactDto?)null);

        var result = await _ctrl.Update("x", new UpdateArtefactRequest { Title = "x" }, default);
        result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task Disable_returns_Ok_or_NotFound()
    {
        _svc.Setup(s => s.DisableAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        (await _ctrl.Disable(new IdBody { Id = "a1" }, default)).Should().BeOfType<OkResult>();

        _svc.Setup(s => s.DisableAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        (await _ctrl.Disable(new IdBody { Id = "x" }, default)).Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task UnpublishArte_returns_Ok()
    {
        _svc.Setup(s => s.UnpublishAsync("a1", false, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _ctrl.UnpublishArte(new IdBody { Id = "a1" }, default);
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task PublishArteToPublic_returns_Ok()
    {
        _svc.Setup(s => s.PublishAsync("a1", true, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _ctrl.PublishArteToPublic(new IdBody { Id = "a1" }, default);
        result.Should().BeOfType<OkResult>();
    }

    // ---------- Download / file content ----------

    [TestMethod]
    public async Task Download_returns_Ok_with_payload()
    {
        _svc.Setup(s => s.GetAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(SampleDto());
        var result = await _ctrl.Download("a1", default);
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Download_returns_NotFound()
    {
        _svc.Setup(s => s.GetAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((ArtefactDto?)null);
        (await _ctrl.Download("x", default)).Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task PDFFileContent_and_aliases_return_Ok()
    {
        _svc.Setup(s => s.GetAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(SampleDto());

        (await _ctrl.PDFFileContent("a1", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.FileContent("a1", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetFileContent("a1", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.EtlFileContent("a1", default)).Should().BeOfType<OkObjectResult>();
    }

    // ---------- Extract / template / student docs ----------

    [TestMethod]
    public async Task Extract_calls_CreateAsync()
    {
        var dto = SampleDto();
        _svc.Setup(s => s.CreateAsync(It.IsAny<CreateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _ctrl.Extract(new CreateArtefactRequest { Title = "Extracted" }, default);
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(dto);
    }

    [TestMethod]
    public async Task Template_upserts_with_template_tag()
    {
        CreateArtefactRequest? captured = null;
        _svc.Setup(s => s.UpsertAsync(It.IsAny<CreateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateArtefactRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(SampleDto());

        await _ctrl.Template(new CreateArtefactRequest { Title = "T" }, default);
        captured!.Tags.Should().Contain("template");
    }

    [TestMethod]
    public async Task UpsertStudentDocument_routes_to_service()
    {
        _svc.Setup(s => s.UpsertStudentDocumentAsync(It.IsAny<CreateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleDto());

        var result = await _ctrl.UpsertStudentDocument(new CreateArtefactRequest { Title = "Aadhar" }, default);
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Applicant_upload_aliases_call_student_document()
    {
        _svc.Setup(s => s.UpsertStudentDocumentAsync(It.IsAny<CreateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleDto());

        (await _ctrl.UploadApplicantDocument(new CreateArtefactRequest { Title = "Doc" }, default))
            .Should().BeOfType<OkObjectResult>();
        (await _ctrl.UploadApplicantPhoto(new CreateArtefactRequest { Title = "Photo" }, default))
            .Should().BeOfType<OkObjectResult>();
        (await _ctrl.UploadApplicantAadhar(new CreateArtefactRequest { Title = "Aadhar" }, default))
            .Should().BeOfType<OkObjectResult>();
        (await _ctrl.UploadApplicantHSCMarksCard(new CreateArtefactRequest { Title = "HSC" }, default))
            .Should().BeOfType<OkObjectResult>();
        (await _ctrl.UploadApplicantSSCMarksCard(new CreateArtefactRequest { Title = "SSC" }, default))
            .Should().BeOfType<OkObjectResult>();
    }

    // ---------- Comment / Attribute / Video ----------

    [TestMethod]
    public async Task AddComment_returns_Ok()
    {
        _svc.Setup(s => s.AddCommentAsync("a1", "hi", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _ctrl.AddComment(new CommentBody { Id = "a1", Text = "hi" }, default);
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task AddAttribute_returns_Ok()
    {
        _svc.Setup(s => s.AddAttributeAsync("a1", "k", "v", 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _ctrl.AddAttribute(new AttributeBody { Id = "a1", Key = "k", Value = "v", ItemId = 1 }, default);
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task VideoLink_returns_Ok()
    {
        _svc.Setup(s => s.AddVideoLinkAsync("a1", "Stream", "https://v", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _ctrl.VideoLink(new VideoLinkBody { Id = "a1", Type = "Stream", Url = "https://v", ItemId = 1 }, default);
        result.Should().BeOfType<OkResult>();
    }

    // ---------- Public URLs ----------

    [TestMethod]
    public async Task UpdatePublicUrl_returns_Ok()
    {
        _svc.Setup(s => s.UpdatePublicUrlAsync("a1", "https://p", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        (await _ctrl.UpdatePublicUrl(new PublicUrlBody { Id = "a1", Url = "https://p" }, default))
            .Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task RemovePublicUrl_returns_Ok()
    {
        _svc.Setup(s => s.RemovePublicUrlAsync("a1", "https://p", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        (await _ctrl.RemovePublicUrl(new PublicUrlBody { Id = "a1", Url = "https://p" }, default))
            .Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task ResetPublicUrl_returns_Ok()
    {
        _svc.Setup(s => s.ResetPublicUrlAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        (await _ctrl.ResetPublicUrl(new IdBody { Id = "a1" }, default)).Should().BeOfType<OkResult>();
    }

    // ---------- Data templates / tags / file names / ETL ----------

    [TestMethod]
    public async Task GetDataTemplates_returns_Ok()
    {
        _svc.Setup(s => s.GetDataTemplatesAsync("live", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtefactDto>());
        (await _ctrl.GetDataTemplates("live", default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetDataTemplatesLive_and_Working()
    {
        _svc.Setup(s => s.GetDataTemplatesAsync("live", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtefactDto>());
        _svc.Setup(s => s.GetDataTemplatesAsync("working", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtefactDto>());

        (await _ctrl.GetDataTemplatesLive(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetDataTemplatesWorking(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task DataTemplatesSave_adds_data_template_tag()
    {
        CreateArtefactRequest? captured = null;
        _svc.Setup(s => s.UpsertAsync(It.IsAny<CreateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateArtefactRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(SampleDto());

        await _ctrl.DataTemplatesSave(new CreateArtefactRequest { Title = "DT" }, default);
        captured!.Tags.Should().Contain("data-template");
    }

    [TestMethod]
    public async Task GetTags_returns_Ok()
    {
        _svc.Setup(s => s.GetTagsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "a" });
        (await _ctrl.GetTags(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task CreateTag_returns_Ok()
    {
        _svc.Setup(s => s.CreateTagAsync("newtag", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { name = "newtag" });
        (await _ctrl.CreateTag(new TagNameBody { Name = "newtag" }, default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetFileNames_returns_Ok()
    {
        _svc.Setup(s => s.GetFileNamesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<object>());
        (await _ctrl.GetFileNames(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetTemplate_and_GetModel_and_GetUrl()
    {
        _svc.Setup(s => s.GetTemplateAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(new { Id = "a1" });
        _svc.Setup(s => s.GetModelAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(new { Id = "a1" });
        _svc.Setup(s => s.GetUrlAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(new { Id = "a1" });
        _svc.Setup(s => s.GetTemplateAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((object?)null);

        (await _ctrl.GetTemplate("a1", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetModel("a1", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetUrl("a1", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetTemplate("x", default)).Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task GenerateEtl_and_RunEtl()
    {
        _svc.Setup(s => s.GenerateEtlStubAsync("student", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { kind = "student" });
        _svc.Setup(s => s.RunEtlStubAsync("a1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { status = "queued" });
        _svc.Setup(s => s.RunEtlStubAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { status = "queued" });

        (await _ctrl.GenerateEtl("student", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.RunEtl(new IdBody { Id = "a1" }, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ExecuteEtl(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task RemoveFile_soft_deletes()
    {
        _svc.Setup(s => s.SoftDeleteAsync("a1", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        (await _ctrl.RemoveFile(new IdBody { Id = "a1" }, default)).Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task View_and_ViewDocument_return_artefact()
    {
        _svc.Setup(s => s.GetAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(SampleDto());
        (await _ctrl.view(new IdBody { Id = "a1" }, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ViewDocument(new IdBody { Id = "a1" }, default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task StudentAttendanceTemplate_returns_File()
    {
        _svc.Setup(s => s.StudentAttendanceTemplateAsync())
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("StudentId,Name\n"));

        var result = await _ctrl.StudentAttendanceTemplate();
        result.Should().BeOfType<FileContentResult>()
            .Which.ContentType.Should().Be("text/csv");
    }

    [TestMethod]
    public async Task GetReports_returns_Ok()
    {
        _svc.Setup(s => s.GetReportsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtefactDto>());
        (await _ctrl.GetReports(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Static_template_endpoints_return_Ok()
    {
        _ctrl.MarksCardTemplate().Should().BeOfType<OkObjectResult>();
        _ctrl.SysAdminPolicy().Should().BeOfType<OkObjectResult>();
        _ctrl.GetEmailTemplate().Should().BeOfType<OkObjectResult>();
        _ctrl.GetEmailTemplateWithoutHF().Should().BeOfType<OkObjectResult>();
        _ctrl.EnableTag().Should().BeOfType<OkObjectResult>();
        _ctrl.DisableTag().Should().BeOfType<OkObjectResult>();
    }
}
