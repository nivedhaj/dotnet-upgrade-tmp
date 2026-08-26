using Ct.Core; using Ct.Stu.Application; using Microsoft.AspNetCore.Mvc;
namespace Ct.Stu.Api.Controllers;
[ApiController,Route("api/stu/[controller]")] public sealed class StudentController(IStudentQueries queries,IStudentCommands commands):ControllerBase
{
 [HttpGet] public async Task<ActionResult<ApiResponse<PagedResult<StudentListItem>>>> Get([FromQuery]string? id,[FromQuery]bool activeStudentsOnly=false,[FromQuery]bool todaysAbsenteesOnly=false,[FromQuery]bool tcStudentsOnly=false,[FromQuery]int page=1,[FromQuery]int per=50,CancellationToken ct=default){if(!string.IsNullOrWhiteSpace(id)){var s=await queries.GetByIdAsync(Guid.Parse(id),ct);return s is null?NotFound():Ok(ApiResponse<StudentDetails>.Ok(s,"Retrieved student"));}var r=await queries.GetAsync(null,activeStudentsOnly,tcStudentsOnly,page,per,ct);return Ok(ApiResponse<PagedResult<StudentListItem>>.Ok(r,"Retrieved students",r.TotalRecords));}
 [HttpGet("all")] public async Task<IActionResult> All([FromQuery]bool includeRTE=false,CancellationToken ct=default)=>Ok(ApiResponse<IReadOnlyList<StudentListItem>>.Ok(await queries.GetAllAsync(includeRTE,ct),$"Retrieved students. RTE included: {includeRTE}"));
 [HttpGet("{id:guid}")] public async Task<IActionResult> ById(Guid id,CancellationToken ct)=> (await queries.GetByIdAsync(id,ct)) is { } s?Ok(ApiResponse<StudentDetails>.Ok(s,"Retrieved student")):NotFound();
 [HttpPost] public async Task<IActionResult> Post(CreateStudentRequest request,CancellationToken ct){var s=await commands.CreateAsync(request,ct);return CreatedAtAction(nameof(ById),new{id=s.Id},s);}
 [HttpPost("enable/{id:guid}")] public async Task<IActionResult> Enable(Guid id,CancellationToken ct)=>await commands.EnableAsync(id,ct)?Ok(id):NotFound();
 [HttpPost("disable/{id:guid}")] public async Task<IActionResult> Disable(Guid id,CancellationToken ct)=>await commands.DisableAsync(id,ct)?Ok(id):NotFound();
 [HttpPost("tc/{id:guid}")] public async Task<IActionResult> Tc(Guid id,CancellationToken ct)=>await commands.TcAsync(id,ct)?Ok(id):NotFound();
 [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id,UpdateStudentRequest request,CancellationToken ct)=>(await commands.UpdateAsync(id,request,ct)) is { } s?Ok(s):NotFound();
}
