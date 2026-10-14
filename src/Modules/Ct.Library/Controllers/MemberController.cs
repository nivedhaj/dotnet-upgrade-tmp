using Ct.Library.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Library.Controllers;

[ApiController]
[Route("api/library/member")]
[Route("api/Member")]
public sealed class MemberController(MemberService members) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await members.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateMemberRequest request, CancellationToken ct)
        => Ok(await members.CreateAsync(request, ct));

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] CreateMemberRequest request, CancellationToken ct)
        => Ok(await members.UpsertAsync(request, ct));

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] LibIdBody body, CancellationToken ct)
        => await members.EnableAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] LibIdBody body, CancellationToken ct)
        => await members.DisableAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("setAttributes")]
    public async Task<IActionResult> SetAttributes([FromBody] AttrBody body, CancellationToken ct)
        => await members.SetAttributesAsync(body.Id, body.Attributes ?? new(), ct) ? Ok() : NotFound();

    [HttpPost("setTags")]
    public async Task<IActionResult> SetTags([FromBody] TagsBody body, CancellationToken ct)
        => await members.SetTagsAsync(body.Id, body.Tags ?? [], ct) ? Ok() : NotFound();

    [HttpPost("job")]
    public async Task<IActionResult> Job(CancellationToken ct) => Ok(await members.JobStubAsync());
}
