using Microsoft.AspNetCore.Mvc;
namespace Ct.Stu.Api.Controllers;
[ApiController,Route("api/stu/[controller]")] public sealed class MonitorController:ControllerBase { [HttpGet("health")] public IActionResult Health()=>Ok(new{service="ct.stu",status="healthy",utc=DateTime.UtcNow}); }
