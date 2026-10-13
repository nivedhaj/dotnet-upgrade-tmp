using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/artefact")]
[Route("api/Artefact")]
public sealed class ArtefactController(ArtefactService artefacts) : ControllerBase
{
    [HttpPost("StudentAttendanceTemplate")]
    public IActionResult StudentAttendanceTemplate()
        => File(artefacts.StudentAttendanceTemplateCsv(), "text/csv", "StudentAttendanceTemplate.csv");
}
