using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/exam")]
[Route("api/Exam")]
public sealed class ExamController(ExamService exams, ExamExtrasService extras) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? courseId, CancellationToken ct)
        => Ok(await exams.GetAllAsync(courseId, ct));

    [HttpGet("getExams")]
    public async Task<IActionResult> GetExams([FromQuery] string? courseId, CancellationToken ct)
        => Ok(await exams.GetAllAsync(courseId, ct));

    [HttpGet("{id:guid}")]
    [HttpGet("getDto")]
    public async Task<IActionResult> GetById([FromQuery] Guid? id, [FromRoute] Guid? idRoute, CancellationToken ct)
    {
        var e = await exams.GetByIdAsync(id ?? idRoute ?? Guid.Empty, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpGet("contents")]
    public async Task<IActionResult> GetContents([FromQuery] Guid id, CancellationToken ct)
    {
        var c = await exams.GetContentsAsync(id, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpGet("getStudents")]
    public async Task<IActionResult> GetStudents([FromQuery] Guid examId, CancellationToken ct)
    {
        var r = await extras.GetStudentsAsync(examId, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("GetStudentScores")]
    [HttpGet("getStudentScores")]
    public async Task<IActionResult> GetStudentScores([FromQuery] Guid examId, [FromQuery] string studentId, CancellationToken ct)
    {
        var r = await extras.GetStudentScoresAsync(examId, studentId, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("facet")]
    public async Task<IActionResult> Facet(CancellationToken ct) => Ok(await extras.FacetAsync(ct));

    [HttpGet("GetGradingDetails")]
    public IActionResult GetGradingDetails() => Ok(new { note = "Use api/acad/grading" });

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateExamRequest request, CancellationToken ct)
        => Ok(await exams.CreateAsync(request, ct));

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] IdRequest request, CancellationToken ct)
        => await exams.EnableAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] IdRequest request, CancellationToken ct)
        => await exams.DisableAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("lock")]
    public async Task<IActionResult> Lock([FromBody] IdRequest request, CancellationToken ct)
        => await exams.LockAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("unlock")]
    public async Task<IActionResult> Unlock([FromBody] IdRequest request, CancellationToken ct)
        => await exams.UnlockAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("rename")]
    public async Task<IActionResult> Rename([FromBody] NameRequest request, CancellationToken ct)
        => await exams.RenameAsync(request.Id, request.Name, ct) ? Ok() : NotFound();

    [HttpPost("setConductedOn")]
    public async Task<IActionResult> SetConductedOn([FromBody] ExamDateRequest request, CancellationToken ct)
        => await exams.SetConductedOnAsync(request.Id, request.Date, ct) ? Ok() : NotFound();

    [HttpPost("setResultDate")]
    public async Task<IActionResult> SetResultDate([FromBody] ExamDateRequest request, CancellationToken ct)
        => await exams.SetResultDateAsync(request.Id, request.Date, ct) ? Ok() : NotFound();

    [HttpPost("setEvaluatedOn")]
    public async Task<IActionResult> SetEvaluatedOn([FromBody] ExamDateRequest request, CancellationToken ct)
        => await exams.SetEvaluatedOnAsync(request.Id, request.Date, ct) ? Ok() : NotFound();

    [HttpPost("addSubject")]
    public async Task<IActionResult> AddSubject([FromBody] ExamSubjectRequest request, CancellationToken ct)
        => await exams.AddSubjectAsync(request.Id, request.SubjectName, request.MaxMarks, request.MinMarks, ct) ? Ok() : NotFound();

    [HttpPost("removeSubject")]
    public async Task<IActionResult> RemoveSubject([FromBody] ExamSubjectRequest request, CancellationToken ct)
        => await exams.RemoveSubjectAsync(request.Id, request.SubjectName, ct) ? Ok() : NotFound();

    [HttpPost("setMarks")]
    public async Task<IActionResult> SetMarks([FromBody] ExamMarksRequest request, CancellationToken ct)
        => await exams.SetStudentMarksAsync(request.Id, request.StudentId, request.SubjectName, request.Marks, ct) ? Ok() : NotFound();

    [HttpPost("setDaysPresent")]
    public async Task<IActionResult> SetDaysPresent([FromBody] ExamAttendanceRequest request, CancellationToken ct)
        => await exams.SetDaysPresentAsync(request.Id, request.StudentId, request.DaysPresent, request.DaysWorked, ct) ? Ok() : NotFound();

    [HttpPost("setRemarks")]
    public async Task<IActionResult> SetRemarks([FromBody] ExamRemarksRequest request, CancellationToken ct)
        => await exams.SetRemarksAsync(request.Id, request.StudentId, request.Remarks, ct) ? Ok() : NotFound();

    [HttpPost("setAttendance")]
    public async Task<IActionResult> SetAttendance([FromBody] ExamRemarksRequest request, CancellationToken ct)
        => await exams.SetAttendanceAsync(request.Id, request.StudentId, request.Remarks, ct) ? Ok() : NotFound();

    [HttpPost("setMinMarks")]
    public async Task<IActionResult> SetMinMarks([FromBody] ExamSubjectRequest request, CancellationToken ct)
        => await exams.SetMinMarksAsync(request.Id, request.SubjectName, request.MinMarks, ct) ? Ok() : NotFound();

    [HttpPost("setSubjectGrading")]
    public async Task<IActionResult> SetSubjectGrading([FromBody] ExamGradingRequest request, CancellationToken ct)
        => await exams.SetSubjectGradingAsync(request.Id, request.SubjectName, request.GradingId, ct) ? Ok() : NotFound();

    [HttpPost("copyContents")]
    public async Task<IActionResult> CopyContents([FromBody] ExamCopyRequest request, CancellationToken ct)
    {
        var e = await exams.CopyContentsAsync(request.SourceId, request.NewName ?? "", ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost("addStudent")]
    public async Task<IActionResult> AddStudent([FromBody] ExamStudentRequest request, CancellationToken ct)
        => await exams.AddStudentAsync(request.Id, request.StudentId, request.Name, ct) ? Ok() : NotFound();

    [HttpPost("widget/studentPerformance")]
    public async Task<IActionResult> GetStudentPerformanceWidgetData([FromBody] StudentIdBody request, CancellationToken ct)
        => Ok(await extras.GetStudentPerformanceWidgetAsync(request.StudentId, ct));

    [HttpPost("generateMarksCard")]
    [HttpPost("getMarksCard")]
    public async Task<IActionResult> GenerateMarksCard([FromBody] MarksCardRequest request, CancellationToken ct)
        => Ok(await extras.GenerateMarksCardAsync(request.ExamId, request.StudentId, ct));

    [HttpPost("enqueMarksCardJob")]
    [HttpPost("enqueEmailMarksCardJob")]
    public async Task<IActionResult> EnqueMarksCardJob([FromBody] MarksCardRequest request, CancellationToken ct)
        => Ok(await extras.EnqueMarksCardJobAsync(request.ExamId, request.StudentId, ct));

    [HttpPost("createExam")]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamRequest request, CancellationToken ct)
        => await Post(request, ct);

    [HttpPost("renameExam")]
    public async Task<IActionResult> RenameExam([FromBody] NameRequest request, CancellationToken ct)
        => await Rename(request, ct);

    [HttpPost("lockExam")]
    public async Task<IActionResult> LockExam([FromBody] IdRequest request, CancellationToken ct)
        => await Lock(request, ct);

    [HttpPost("unlockExam")]
    public async Task<IActionResult> UnlockExam([FromBody] IdRequest request, CancellationToken ct)
        => await Unlock(request, ct);

    [HttpPost("SetTotalMaxMarks")]
    public async Task<IActionResult> SetTotalMaxMarks([FromBody] ExamSubjectRequest request, CancellationToken ct)
        => await exams.AddSubjectAsync(request.Id, request.SubjectName, request.MaxMarks, request.MinMarks, ct) ? Ok() : NotFound();

    [HttpPost("setMaxMarks")]
    public async Task<IActionResult> SetMaxMarks([FromBody] ExamSubjectRequest request, CancellationToken ct)
        => await SetTotalMaxMarks(request, ct);

    [HttpPost("setScheduledOn")]
    public async Task<IActionResult> SetScheduledOn([FromBody] ExamDateRequest request, CancellationToken ct)
        => await SetConductedOn(request, ct);

    [HttpGet("getExamContents")]
    public async Task<IActionResult> GetExamContents([FromQuery] Guid id, CancellationToken ct)
        => await GetContents(id, ct);

    [HttpPost("setDaysWorked")]
    public async Task<IActionResult> SetDaysWorked([FromBody] ExamAttendanceRequest request, CancellationToken ct)
        => await SetDaysPresent(request, ct);

    [HttpPost("subjectGrading")]
    public async Task<IActionResult> SubjectGrading([FromBody] ExamGradingRequest request, CancellationToken ct)
        => await SetSubjectGrading(request, ct);

    [HttpPost("AddChildSubject")]
    public async Task<IActionResult> AddChildSubject([FromBody] ExamSubjectRequest request, CancellationToken ct)
        => await AddSubject(request, ct);

    [HttpPost("RemoveChildSubject")]
    public async Task<IActionResult> RemoveChildSubject([FromBody] ExamSubjectRequest request, CancellationToken ct)
        => await RemoveSubject(request, ct);

    [HttpGet("ForStudent")]
    public async Task<IActionResult> ForStudent([FromQuery] string studentId, CancellationToken ct)
        => Ok(await extras.GetStudentPerformanceWidgetAsync(studentId, ct));

    [HttpPost("CopyExamContents")]
    public async Task<IActionResult> CopyExamContents([FromBody] ExamCopyRequest request, CancellationToken ct)
        => await CopyContents(request, ct);

    [HttpGet("getStudentsToEvaluate")]
    public async Task<IActionResult> GetStudentsToEvaluate([FromQuery] Guid examId, CancellationToken ct)
        => await GetStudents(examId, ct);

    [HttpPost("attendance")]
    public async Task<IActionResult> Attendance([FromBody] ExamRemarksRequest request, CancellationToken ct)
        => await SetAttendance(request, ct);

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate([FromBody] ExamMarksRequest request, CancellationToken ct)
        => await SetMarks(request, ct);

    [HttpPost("daysPresent")]
    public async Task<IActionResult> DaysPresent([FromBody] ExamAttendanceRequest request, CancellationToken ct)
        => await SetDaysPresent(request, ct);

    [HttpPost("remarks")]
    public async Task<IActionResult> ExamRemarks([FromBody] ExamRemarksRequest request, CancellationToken ct)
        => await SetRemarks(request, ct);

    [HttpPost("applyEventsAsync")]
    public async Task<IActionResult> ApplyEventsAsync([FromBody] List<ExamMarksRequest> events, CancellationToken ct)
    {
        foreach (var e in events ?? [])
            await exams.SetStudentMarksAsync(e.Id, e.StudentId, e.SubjectName, e.Marks, ct);
        return Ok(new { count = events?.Count ?? 0 });
    }

    [HttpGet("GetExamsCoursewise")]
    public async Task<IActionResult> GetExamsCoursewise([FromQuery] string? courseId, CancellationToken ct)
        => Ok(await exams.GetAllAsync(courseId, ct));

    // GetExams already via getExams route on Get
}


public sealed class ExamDateRequest { public required Guid Id { get; set; } public required DateTime Date { get; set; } }
public sealed class ExamSubjectRequest
{
    public required Guid Id { get; set; }
    public required string SubjectName { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal MinMarks { get; set; }
}
public sealed class ExamMarksRequest
{
    public required Guid Id { get; set; }
    public required string StudentId { get; set; }
    public required string SubjectName { get; set; }
    public decimal? Marks { get; set; }
}
public sealed class ExamAttendanceRequest
{
    public required Guid Id { get; set; }
    public required string StudentId { get; set; }
    public int DaysPresent { get; set; }
    public int? DaysWorked { get; set; }
}
public sealed class ExamRemarksRequest
{
    public required Guid Id { get; set; }
    public required string StudentId { get; set; }
    public required string Remarks { get; set; }
}
public sealed class ExamGradingRequest
{
    public required Guid Id { get; set; }
    public required string SubjectName { get; set; }
    public required string GradingId { get; set; }
}
public sealed class ExamCopyRequest
{
    public required Guid SourceId { get; set; }
    public string? NewName { get; set; }
}
public sealed class ExamStudentRequest
{
    public required Guid Id { get; set; }
    public required string StudentId { get; set; }
    public string? Name { get; set; }
}
