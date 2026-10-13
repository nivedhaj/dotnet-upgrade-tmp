using Ct.Arte.Application.Dtos;
using Ct.Arte.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Arte.Controllers;

[ApiController]
[Route("api/arte/artefact")]
[Route("api/Artefact")]
public sealed class ArtefactController(IArtefactService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await svc.SearchAsync(new ArtefactSearchRequest { Search = search, Skip = skip, Take = take }, ct));

    [HttpGet("getOnQry")]
    public async Task<IActionResult> GetOnQry([FromQuery] string? search, CancellationToken ct)
        => Ok(await svc.SearchAsync(new ArtefactSearchRequest { Search = search }, ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var a = await svc.GetAsync(id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost]
    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => Ok(await svc.UpsertAsync(request, ct));

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromQuery] string id, [FromBody] UpdateArtefactRequest request, CancellationToken ct)
    {
        var a = await svc.UpdateAsync(id, request, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost("updateReport")]
    public async Task<IActionResult> UpdateReport([FromQuery] string id, [FromBody] UpdateArtefactRequest request, CancellationToken ct)
        => await Update(id, request, ct);

    [HttpPost("updateTags")]
    public async Task<IActionResult> UpdateTags([FromBody] TagsBody body, CancellationToken ct)
        => await svc.UpdateTagsAsync(body.Id, body.Tags, ct) ? Ok() : NotFound();

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] IdBody body, CancellationToken ct)
        => await svc.EnableAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] IdBody body, CancellationToken ct)
        => await svc.DisableAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromBody] IdBody body, CancellationToken ct)
        => await svc.PublishAsync(body.Id, false, ct) ? Ok() : NotFound();

    [HttpPost("publishArtefact")]
    public async Task<IActionResult> PublishArte([FromBody] IdBody body, CancellationToken ct)
        => await svc.PublishAsync(body.Id, false, ct) ? Ok() : NotFound();

    [HttpPost("publishArtefactToPublic")]
    public async Task<IActionResult> PublishArteToPublic([FromBody] IdBody body, CancellationToken ct)
        => await svc.PublishAsync(body.Id, true, ct) ? Ok() : NotFound();

    [HttpPost("unpublishArtefact")]
    public async Task<IActionResult> UnpublishArte([FromBody] IdBody body, CancellationToken ct)
        => await svc.UnpublishAsync(body.Id, false, ct) ? Ok() : NotFound();

    [HttpPost("unpublishPublicArtefact")]
    public async Task<IActionResult> UnpublishPublicArte([FromBody] IdBody body, CancellationToken ct)
        => await svc.UnpublishAsync(body.Id, true, ct) ? Ok() : NotFound();

    [HttpGet("dataTemplates")]
    public async Task<IActionResult> GetDataTemplates([FromQuery] string? mode, CancellationToken ct)
        => Ok(await svc.GetDataTemplatesAsync(mode, ct));

    [HttpGet("dataTemplatesLive")]
    public async Task<IActionResult> GetDataTemplatesLive(CancellationToken ct)
        => Ok(await svc.GetDataTemplatesAsync("live", ct));

    [HttpGet("dataTemplatesWorking")]
    public async Task<IActionResult> GetDataTemplatesWorking(CancellationToken ct)
        => Ok(await svc.GetDataTemplatesAsync("working", ct));

    [HttpGet("getDataTemplatesByName")]
    public async Task<IActionResult> GetDataTemplatesByName([FromQuery] string? name, CancellationToken ct)
        => Ok(await svc.GetDataTemplatesAsync(name, ct));

    [HttpPost("dataTemplatesSave")]
    public async Task<IActionResult> DataTemplatesSave([FromBody] CreateArtefactRequest request, CancellationToken ct)
    {
        request.Tags ??= [];
        if (!request.Tags.Contains("data-template")) request.Tags.Add("data-template");
        return Ok(await svc.UpsertAsync(request, ct));
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(CancellationToken ct) => Ok(await svc.GetReportsAsync(ct));

    [HttpGet("getTemplate")]
    public async Task<IActionResult> GetTemplate([FromQuery] string id, CancellationToken ct)
    {
        var r = await svc.GetTemplateAsync(id, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("getModel")]
    public async Task<IActionResult> GetModel([FromQuery] string id, CancellationToken ct)
    {
        var r = await svc.GetModelAsync(id, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("getUrl")]
    public async Task<IActionResult> GetUrl([FromQuery] string id, CancellationToken ct)
    {
        var r = await svc.GetUrlAsync(id, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("getFileUrl")]
    public async Task<IActionResult> GetFileUrl([FromQuery] string id, CancellationToken ct)
        => await GetUrl(id, ct);

    [HttpGet("marksCardTemplate")]
    public IActionResult MarksCardTemplate() => Ok(new { html = "<h2>Marks Card Template</h2>" });

    [HttpGet("sysAdminPolicy")]
    public IActionResult SysAdminPolicy() => Ok(new { policy = "sysadmin", note = "Configure via host policy engine." });

    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string id, CancellationToken ct)
    {
        var a = await svc.GetAsync(id, ct);
        if (a is null) return NotFound();
        return Ok(new { a.Id, a.Url, a.FileName, a.MimeType, download = a.Url });
    }

    [HttpGet("downloadWithTimestamp")]
    public async Task<IActionResult> DownloadWithTimestamp([FromQuery] string id, CancellationToken ct)
        => await Download(id, ct);

    [HttpGet("pdfFileContent")]
    public async Task<IActionResult> PDFFileContent([FromQuery] string id, CancellationToken ct)
    {
        var a = await svc.GetAsync(id, ct);
        return a is null ? NotFound() : Ok(new { a.Id, a.FileName, a.MimeType, contentUrl = a.Url });
    }

    [HttpGet("fileContent")]
    public async Task<IActionResult> FileContent([FromQuery] string id, CancellationToken ct)
        => await PDFFileContent(id, ct);

    [HttpGet("getFileContent")]
    public async Task<IActionResult> GetFileContent([FromQuery] string id, CancellationToken ct)
        => await PDFFileContent(id, ct);

    [HttpGet("etlFileContent")]
    public async Task<IActionResult> EtlFileContent([FromQuery] string id, CancellationToken ct)
        => await PDFFileContent(id, ct);

    [HttpPost("extract")]
    public async Task<IActionResult> Extract([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => Ok(await svc.CreateAsync(request, ct));

    [HttpPost("uploadAndExtract")]
    public async Task<IActionResult> UploadAndExtract([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => await Extract(request, ct);

    [HttpPost("studentAttendanceTemplate")]
    public async Task<IActionResult> StudentAttendanceTemplate()
        => File(await svc.StudentAttendanceTemplateAsync(), "text/csv", "StudentAttendanceTemplate.csv");

    [HttpPost("template")]
    public async Task<IActionResult> Template([FromBody] CreateArtefactRequest request, CancellationToken ct)
    {
        request.Tags ??= [];
        if (!request.Tags.Contains("template")) request.Tags.Add("template");
        return Ok(await svc.UpsertAsync(request, ct));
    }

    [HttpPost("view")]
    public async Task<IActionResult> view([FromBody] IdBody body, CancellationToken ct)
    {
        var a = await svc.GetAsync(body.Id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost("removeFile")]
    public async Task<IActionResult> RemoveFile([FromBody] IdBody body, CancellationToken ct)
    {
        await svc.SoftDeleteAsync(body.Id, ct);
        return Ok();
    }

    [HttpGet("fileNames")]
    public async Task<IActionResult> GetFileNames(CancellationToken ct) => Ok(await svc.GetFileNamesAsync(ct));

    [HttpGet("emailTemplate")]
    public IActionResult GetEmailTemplate() => Ok(new { html = "<p>Email template</p>" });

    [HttpGet("emailTemplateWithoutHF")]
    public IActionResult GetEmailTemplateWithoutHF() => Ok(new { html = "<p>Email template</p>" });

    [HttpPost("viewMarksCard")]
    public async Task<IActionResult> ViewMarksCard([FromBody] IdBody body, CancellationToken ct)
    {
        var a = await svc.GetAsync(body.Id, ct);
        return a is null ? NotFound() : Ok(new { a.Id, html = a.Description ?? "<h2>Marks Card</h2>" });
    }

    [HttpGet("generateEtl")]
    public async Task<IActionResult> GenerateEtl([FromQuery] string kind = "student", CancellationToken ct = default)
        => Ok(await svc.GenerateEtlStubAsync(kind, ct));

    [HttpPost("runEtl")]
    public async Task<IActionResult> RunEtl([FromBody] IdBody? body, CancellationToken ct)
        => Ok(await svc.RunEtlStubAsync(body?.Id, ct));

    [HttpPost("executeAllEtlsInDir")]
    public async Task<IActionResult> ExecuteEtl(CancellationToken ct)
        => Ok(await svc.RunEtlStubAsync(null, ct));

    [HttpPost("uploadApplicantDocument")]
    public async Task<IActionResult> UploadApplicantDocument([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => Ok(await svc.UpsertStudentDocumentAsync(request, ct));

    [HttpPost("applicantPhoto")]
    public async Task<IActionResult> UploadApplicantPhoto([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => await UploadApplicantDocument(request, ct);

    [HttpPost("applicantAadhar")]
    public async Task<IActionResult> UploadApplicantAadhar([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => await UploadApplicantDocument(request, ct);

    [HttpPost("applicantHSCMarksCard")]
    public async Task<IActionResult> UploadApplicantHSCMarksCard([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => await UploadApplicantDocument(request, ct);

    [HttpPost("applicantSSCMarksCard")]
    public async Task<IActionResult> UploadApplicantSSCMarksCard([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => await UploadApplicantDocument(request, ct);

    [HttpPost("upsertStudentDocument")]
    public async Task<IActionResult> UpsertStudentDocument([FromBody] CreateArtefactRequest request, CancellationToken ct)
        => await UploadApplicantDocument(request, ct);

    [HttpPost("viewDocument")]
    public async Task<IActionResult> ViewDocument([FromBody] IdBody body, CancellationToken ct)
    {
        var a = await svc.GetAsync(body.Id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost("tag")]
    public async Task<IActionResult> CreateTag([FromBody] TagNameBody body, CancellationToken ct)
        => Ok(await svc.CreateTagAsync(body.Name, ct));

    [HttpPost("contentTag")]
    public async Task<IActionResult> CreateContentTag([FromBody] TagNameBody body, CancellationToken ct)
        => await CreateTag(body, ct);

    [HttpPost("comment")]
    public async Task<IActionResult> AddComment([FromBody] CommentBody body, CancellationToken ct)
        => await svc.AddCommentAsync(body.Id, body.Text, body.Html, ct) ? Ok() : NotFound();

    [HttpPost("attribute")]
    public async Task<IActionResult> AddAttribute([FromBody] AttributeBody body, CancellationToken ct)
        => await svc.AddAttributeAsync(body.Id, body.Key, body.Value, body.ItemId, ct) ? Ok() : NotFound();

    [HttpPost("videoLink")]
    public async Task<IActionResult> VideoLink([FromBody] VideoLinkBody body, CancellationToken ct)
        => await svc.AddVideoLinkAsync(body.Id, body.Type, body.Url, body.ItemId, ct) ? Ok() : NotFound();

    [HttpGet("tags")]
    [HttpGet("contentTags")]
    public async Task<IActionResult> GetTags(CancellationToken ct) => Ok(await svc.GetTagsAsync(ct));

    [HttpPost("updateTag")]
    public async Task<IActionResult> UpdateTag([FromBody] TagNameBody body, CancellationToken ct)
        => Ok(await svc.CreateTagAsync(body.Name, ct));

    [HttpPost("updateContentTag")]
    public async Task<IActionResult> UpdateContentTag([FromBody] TagNameBody body, CancellationToken ct)
        => await UpdateTag(body, ct);

    [HttpPost("enableTag")]
    public IActionResult EnableTag() => Ok(new { ok = true });

    [HttpPost("disableTag")]
    public IActionResult DisableTag() => Ok(new { ok = true });

    [HttpPost("enableContentTag")]
    public IActionResult EnableContentTag() => Ok(new { ok = true });

    [HttpPost("disableContentTag")]
    public IActionResult DisableContentTag() => Ok(new { ok = true });

    [HttpGet("widget/artefactBandwidth")]
    public async Task<IActionResult> GetArtefactBandwidthWidgetData(CancellationToken ct)
        => Ok(await svc.WidgetBandwidthAsync(ct));

    [HttpGet("widget/artefactCount")]
    public async Task<IActionResult> GetArtefactCountWidgetData(CancellationToken ct)
        => Ok(await svc.WidgetCountAsync(ct));

    [HttpGet("widget/sizeBuckets")]
    public async Task<IActionResult> GetWidgetSizeBucketsData(CancellationToken ct)
        => Ok(await svc.WidgetSizeBucketsAsync(ct));

    [HttpGet("widget/artefactAuthorCount")]
    public async Task<IActionResult> GetWidgetArtefactAuthorCountData(CancellationToken ct)
        => Ok(await svc.WidgetAuthorCountAsync(ct));

    [HttpGet("widget/fileCount")]
    public async Task<IActionResult> GetWidgetFileCount(CancellationToken ct)
        => Ok(await svc.WidgetFileCountAsync(ct));

    [HttpPost("updatePublicUrl")]
    public async Task<IActionResult> UpdatePublicUrl([FromBody] PublicUrlBody body, CancellationToken ct)
        => await svc.UpdatePublicUrlAsync(body.Id, body.Url, ct) ? Ok() : NotFound();

    [HttpPost("removePublicUrl")]
    public async Task<IActionResult> RemovePublicUrl([FromBody] PublicUrlBody body, CancellationToken ct)
        => await svc.RemovePublicUrlAsync(body.Id, body.Url, ct) ? Ok() : NotFound();

    [HttpPost("resetPublicUrl")]
    public async Task<IActionResult> ResetPublicUrl([FromBody] IdBody body, CancellationToken ct)
        => await svc.ResetPublicUrlAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpGet("getEtlFileNames")]
    public async Task<IActionResult> GetEtlFileNames(CancellationToken ct)
        => Ok(await svc.GetFileNamesAsync(ct));
}
