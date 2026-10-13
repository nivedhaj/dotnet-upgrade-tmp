using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/report")]
[Route("api/Report")]
public sealed class ReportController(AcadReportService reports, ReportExtrasService extras, AssessmentService assessments) : ControllerBase
{
    [HttpPost("getCourseReport")]
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourseListReport(CancellationToken ct) => Ok(await reports.CourseListAsync(ct));

    [HttpPost("todaysEvents")]
    public async Task<IActionResult> TodaysEvents(CancellationToken ct) => Ok(await reports.TodaysEventsAsync(ct));

    [HttpPost("holidayList")]
    public async Task<IActionResult> HolidayList(CancellationToken ct) => Ok(await reports.HolidayListAsync(ct));

    [HttpPost("upcomingEvents")]
    public async Task<IActionResult> UpcomingEvents(CancellationToken ct) => Ok(await reports.UpcomingEventsAsync(ct));

    [HttpPost("consolidatedMarks")]
    [HttpPost("consolidatedReport")]
    public async Task<IActionResult> ConsolidatedMarks([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await reports.ConsolidatedMarksAsync(request.Id, ct));

    [HttpPost("consolidatedReport")]
    public async Task<IActionResult> ConsolidatedReport([FromBody] IdRequest request, CancellationToken ct)
        => await ConsolidatedMarks(request, ct);

    [HttpPost("rankList")]
    public async Task<IActionResult> RankList([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await reports.RankListAsync(request.Id, ct));

    [HttpPost("resultAtAGlance")]
    [HttpPost("resultAtAGlanceSummary")]
    [HttpPost("resultAnalysis")]
    [HttpPost("coursewiseResultAnalysis")]
    [HttpPost("coursewiseExamwisePerformanceAsJson")]
    public async Task<IActionResult> ResultAtAGlance([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await reports.ResultAtAGlanceAsync(request.Id, ct));

    [HttpPost("coursewiseResultAnalysis")]
    public async Task<IActionResult> CoursewiseResultAnalysis([FromBody] IdRequest request, CancellationToken ct)
        => await ResultAtAGlance(request, ct);

    [HttpPost("coursewiseExamwisePerformanceAsJson")]
    public async Task<IActionResult> CoursewiseResultAnalysisAsJson([FromBody] IdRequest request, CancellationToken ct)
        => await ResultAtAGlance(request, ct);

    [HttpPost("resultAnalysis")]
    public async Task<IActionResult> ResultAnalysis([FromBody] IdRequest request, CancellationToken ct)
        => await ResultAtAGlance(request, ct);

    [HttpPost("resultAtAGlanceSummary")]
    public async Task<IActionResult> ResultAtAGlanceSummary([FromBody] IdRequest request, CancellationToken ct)
        => await ResultAtAGlance(request, ct);

    [HttpPost("subjectTopScorers")]
    public async Task<IActionResult> SubjectTopScorers([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await reports.SubjectTopScorersAsync(request.Id, ct));

    [HttpPost("topTen")]
    public async Task<IActionResult> TopTen([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await reports.TopTenAsync(request.Id, ct));

    [HttpPost("examAttendance")]
    public async Task<IActionResult> ExamAttendance([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await reports.ExamAttendanceAsync(request.Id, ct));

    [HttpPost("allAPlus")]
    public async Task<IActionResult> AllAPlus([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await reports.ConsolidatedMarksAsync(request.Id, ct));

    [HttpPost("examList")]
    [HttpPost("examListAsJson")]
    public async Task<IActionResult> ExamList(CancellationToken ct) => Ok(await extras.ExamListAsync(ct));

    [HttpPost("examListAsJson")]
    public async Task<IActionResult> ExamListAsJson(CancellationToken ct) => await ExamList(ct);

    [HttpPost("examwiseFailures")]
    [HttpPost("failures")]
    public async Task<IActionResult> Failures([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await extras.FailuresAsync(request.Id, ct));

    [HttpPost("examwiseFailures")]
    public async Task<IActionResult> ExamwiseFailures([FromBody] IdRequest request, CancellationToken ct)
        => await Failures(request, ct);

    [HttpPost("fullMarks")]
    public async Task<IActionResult> FullMarks([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await extras.FullMarksAsync(request.Id, ct));

    [HttpPost("gradewiseStatistics")]
    [HttpGet("gradewiseStatisticsAsJson")]
    public async Task<IActionResult> GradewiseStatistics([FromBody] IdRequest? request, [FromQuery] Guid? examId, CancellationToken ct)
        => Ok(await extras.GradewiseStatisticsAsync(request?.Id ?? examId ?? Guid.Empty, ct));

    [HttpGet("gradewiseStatisticsAsJson")]
    public async Task<IActionResult> GradewiseStatisticsAsJson([FromBody] IdRequest? request, [FromQuery] Guid? examId, CancellationToken ct)
        => await GradewiseStatistics(request, examId, ct);

    [HttpPost("subjectAverage")]
    public async Task<IActionResult> SubjectAverage([FromBody] IdRequest request, CancellationToken ct)
        => Ok(await extras.SubjectAverageAsync(request.Id, ct));

    [HttpPost("studentPerformance")]
    public async Task<IActionResult> StudentPerformance([FromBody] StudentIdBody request, CancellationToken ct)
        => Ok(await extras.StudentPerformanceAsync(request.StudentId, ct));

    [HttpPost("studentPerformanceCSV")]
    public async Task<IActionResult> StudentPerformanceCSV([FromBody] StudentIdBody request, CancellationToken ct)
    {
        var csv = await extras.StudentPerformanceCsvAsync(request.StudentId, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "student-performance.csv");
    }

    [HttpPost("GetMarksCard")]
    [HttpPost("getMarksCard")]
    public async Task<IActionResult> GetMarksCard([FromBody] MarksCardRequest request, CancellationToken ct)
        => Ok(await extras.MarksCardAsync(request.ExamId, request.StudentId, ct));

    [HttpPost("hallTicket")]
    public async Task<IActionResult> HallTicket([FromBody] MarksCardRequest request, CancellationToken ct)
        => Ok(await extras.HallTicketAsync(request.ExamId, request.StudentId, ct));

    [HttpPost("noDueCertificate")]
    public async Task<IActionResult> NoDueCertificate([FromBody] StudentIdBody request, CancellationToken ct)
        => Ok(await extras.NoDueCertificateAsync(request.StudentId, ct));

    [HttpPost("assessment")]
    public async Task<IActionResult> Assessment(CancellationToken ct)
        => Ok(await assessments.GetAllAsync(false, ct));
}

public sealed class StudentIdBody { public required string StudentId { get; set; } }
public sealed class MarksCardRequest { public required Guid ExamId { get; set; } public required string StudentId { get; set; } }
