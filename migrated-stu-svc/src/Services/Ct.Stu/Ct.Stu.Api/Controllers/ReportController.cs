using Ct.Stu.Application; using Microsoft.AspNetCore.Mvc;
namespace Ct.Stu.Api.Controllers;
[ApiController,Route("api/stu/[controller]")] public sealed class ReportController(IStudentQueries queries):ControllerBase
{ [HttpGet("student-fee-summary")] public async Task<IActionResult> StudentFeeSummary([FromQuery]Guid? studentId,CancellationToken ct)=>Ok(await queries.GetFeeSummaryAsync(studentId,ct)); }
