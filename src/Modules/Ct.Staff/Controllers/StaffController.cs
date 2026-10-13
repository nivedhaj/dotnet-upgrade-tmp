using Ct.Staff.Application;
using Ct.Staff.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Staff.Controllers;

[ApiController]
[Route("api/staff")]
[Route("api/Staff")]
public sealed class StaffController(StaffService staff) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await staff.GetAllAsync(ct));

    [HttpGet("getById")]
    public async Task<IActionResult> GetById([FromQuery] Guid id, CancellationToken ct)
    {
        var s = await staff.GetByIdAsync(id, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpGet("getByEmail")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email, CancellationToken ct)
    {
        var s = await staff.GetByEmailAsync(email, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request, CancellationToken ct)
        => Ok(await staff.CreateAsync(request, ct));

    [HttpPost("update")]
    public async Task<IActionResult> UpdateStaff([FromQuery] Guid id, [FromBody] CreateStaffRequest request, CancellationToken ct)
    {
        var s = await staff.UpdateAsync(id, request, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] StaffIdBody body, CancellationToken ct)
        => await staff.EnableAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] StaffIdBody body, CancellationToken ct)
        => await staff.DisableAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("assignInstitutes")]
    public async Task<IActionResult> AssignInstitutes([FromBody] AssignListBody body, CancellationToken ct)
        => await staff.AssignInstitutesAsync(body.Id, body.Items ?? [], ct) ? Ok() : NotFound();

    [HttpPost("assignSubjects")]
    public async Task<IActionResult> AssignSubjects([FromBody] AssignListBody body, CancellationToken ct)
        => await staff.AssignSubjectsAsync(body.Id, body.Items ?? [], ct) ? Ok() : NotFound();

    [HttpPost("setAlias")]
    public async Task<IActionResult> SetAlias([FromBody] StringFieldBody body, CancellationToken ct)
        => await staff.SetAliasAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("setClassTeacher")]
    public async Task<IActionResult> setClassTeacher([FromBody] ClassTeacherBody body, CancellationToken ct)
        => await staff.SetClassTeacherAsync(body.Id, body.CourseId, body.Section, ct) ? Ok() : NotFound();

    [HttpPost("setPrimaryEmail")]
    public async Task<IActionResult> setPrimaryEmail([FromBody] StringFieldBody body, CancellationToken ct)
        => await staff.SetPrimaryEmailAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("setPrimaryMobile")]
    public async Task<IActionResult> SetPrimaryMobile([FromBody] StringFieldBody body, CancellationToken ct)
        => await staff.SetPrimaryMobileAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpGet("timeTable")]
    public async Task<IActionResult> Timetable([FromQuery] Guid id, CancellationToken ct)
    {
        var tt = await staff.GetTimetableAsync(id, ct);
        return tt is null ? NotFound() : Ok(tt);
    }

    [HttpPost("timetable")]
    public async Task<IActionResult> Timetable([FromBody] TimetableBody body, CancellationToken ct)
        => await staff.SetTimetableAsync(body.Id, body.Slots ?? [], ct) ? Ok() : NotFound();

    [HttpPost("forInstitute")]
    public async Task<IActionResult> ForInstitute([FromBody] InstBody body, CancellationToken ct)
        => Ok(await staff.ForInstituteAsync(body.InstId, ct));

    [HttpGet("facet")]
    public async Task<IActionResult> Facet(CancellationToken ct) => Ok(await staff.FacetAsync(ct));

    [HttpPost("sms/staff")]
    public async Task<IActionResult> SMSStaff([FromBody] RecipientFilterBody? body, CancellationToken ct)
        => Ok(await staff.GetSmsRecipientsAsync(body?.Teaching, body?.NonTeaching, body?.Management, ct));

    [HttpPost("smsRecipients")]
    public async Task<IActionResult> SMSRecipients([FromBody] RecipientFilterBody? body, CancellationToken ct)
        => Ok(await staff.GetSmsRecipientsAsync(body?.Teaching, body?.NonTeaching, body?.Management, ct));

    [HttpPost("email/staff")]
    public async Task<IActionResult> EmailStaff([FromBody] RecipientFilterBody? body, CancellationToken ct)
        => Ok(await staff.GetEmailRecipientsAsync(body?.Teaching, body?.NonTeaching, body?.Management, ct));

    [HttpPost("recipients")]
    public async Task<IActionResult> SubscriptionRecipients([FromBody] RecipientFilterBody? body, CancellationToken ct)
        => Ok(await staff.GetEmailRecipientsAsync(body?.Teaching, body?.NonTeaching, body?.Management, ct));

    [HttpPost("updateStaffRefId")]
    public async Task<IActionResult> AssignStudentId([FromBody] StringFieldBody body, CancellationToken ct)
        => await staff.AssignStudentIdAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("updateGender")]
    public async Task<IActionResult> UpdateGender([FromBody] StringFieldBody body, CancellationToken ct)
        => await staff.UpdateGenderAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("cache")]
    public IActionResult Cache() => Ok(new { cached = true });

    [HttpPost("employ")]
    public async Task<IActionResult> Employ([FromBody] EmployBody body, CancellationToken ct)
        => await staff.EmployAsync(body.Id, body.InstId, body.Designation, body.RefId, ct) ? Ok() : NotFound();

    [HttpPost("updateDesignation")]
    public async Task<IActionResult> UpdateDesignation([FromBody] DesignationBody body, CancellationToken ct)
        => await staff.UpdateDesignationAsync(body.Id, body.Designation, body.InstId, ct) ? Ok() : NotFound();

    [HttpPost("salaryAdd")]
    public async Task<IActionResult> SalaryAdd([FromBody] SalaryBody body, CancellationToken ct)
        => await staff.SalaryAddAsync(body.Id, body.InstId ?? "", body.Earnings ?? [], body.Deductions ?? [], body.Date, false, ct) ? Ok() : NotFound();

    [HttpPost("salaryUpdate")]
    public async Task<IActionResult> SalaryUpdate([FromBody] SalaryBody body, CancellationToken ct)
        => await staff.SalaryUpdateAsync(body.Id, body.SalaryId, body.Earnings, body.Deductions, ct) ? Ok() : NotFound();

    [HttpPost("salarydelete")]
    public async Task<IActionResult> SalaryDelete([FromBody] SalaryBody body, CancellationToken ct)
        => await staff.SalaryDeleteAsync(body.Id, body.SalaryId, ct) ? Ok() : NotFound();

    [HttpPost("addPayslip")]
    public async Task<IActionResult> AddPayslip([FromBody] SalaryBody body, CancellationToken ct)
        => await staff.SalaryAddAsync(body.Id, body.InstId ?? "", body.Earnings ?? [], body.Deductions ?? [], body.Date, true, ct) ? Ok() : NotFound();

    [HttpPost("verifyPayslip")]
    public async Task<IActionResult> VerifyPayslip([FromBody] SalaryBody body, CancellationToken ct)
    {
        var p = await staff.GetPayslipAsync(body.Id, body.SalaryId == 0 ? null : body.SalaryId, ct);
        return p is null ? NotFound() : Ok(new { verified = true, payslip = p });
    }


    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance([FromQuery] Guid? staffId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await staff.GetAttendanceAsync(staffId, from, to, ct));

    [HttpPost("attendance")]
    public async Task<IActionResult> RecordAttendance([FromBody] AttendanceBody body, CancellationToken ct)
        => await staff.RecordAttendanceAsync(body.Id, body.Date, body.Status, body.SessionNumber, ct) ? Ok() : NotFound();

    [HttpGet("count/institutewise")]
    public async Task<IActionResult> GetStaffCountInstitutewise(CancellationToken ct)
        => Ok(await staff.GetStaffCountInstitutewiseAsync(ct));

    [HttpGet("count/instituteGenderwise")]
    public async Task<IActionResult> GetStaffCountInstituteGenderwise(CancellationToken ct)
        => Ok(await staff.GetStaffCountInstituteGenderwiseAsync(ct));

    [HttpPost("applyEvents")]
    public async Task<IActionResult> ApplyEvents([FromBody] List<CreateStaffRequest> events, CancellationToken ct)
    {
        var results = new List<object>();
        foreach (var e in events ?? [])
            results.Add(await staff.CreateAsync(e, ct));
        return Ok(new { count = results.Count, results });
    }
}

public sealed class StaffIdBody { public required Guid Id { get; set; } }
public sealed class AssignListBody { public required Guid Id { get; set; } public List<string>? Items { get; set; } }
public sealed class StringFieldBody { public required Guid Id { get; set; } public required string Value { get; set; } }
public sealed class ClassTeacherBody { public required Guid Id { get; set; } public string? CourseId { get; set; } public string? Section { get; set; } }
public sealed class TimetableBody { public required Guid Id { get; set; } public List<TtSlot>? Slots { get; set; } }
public sealed class InstBody { public required string InstId { get; set; } }
public sealed class EmployBody { public required Guid Id { get; set; } public required string InstId { get; set; } public string? Designation { get; set; } public string? RefId { get; set; } }
public sealed class DesignationBody { public required Guid Id { get; set; } public required string Designation { get; set; } public string? InstId { get; set; } }
public sealed class SalaryBody
{
    public required Guid Id { get; set; }
    public int SalaryId { get; set; }
    public string? InstId { get; set; }
    public List<SalaryItem>? Earnings { get; set; }
    public List<SalaryItem>? Deductions { get; set; }
    public DateTime? Date { get; set; }
}
public sealed class AttendanceBody
{
    public required Guid Id { get; set; }
    public required DateTime Date { get; set; }
    public required string Status { get; set; }
    public int SessionNumber { get; set; }
}
public sealed class RecipientFilterBody
{
    public List<string>? Teaching { get; set; }
    public List<string>? NonTeaching { get; set; }
    public List<string>? Management { get; set; }
}
