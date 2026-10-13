using System.Text;
using Ct.Acad.Domain;
using Ct.Acad.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Acad.Application;

/// <summary>Remaining original Acad surface not covered by primary services.</summary>
public sealed class CourseExtrasService(AcadDbContext db, IAcadUserContext user, CourseService courses)
{
    public async Task<object?> UpdateCourseAsync(Guid id, CreateCourseRequest req, CancellationToken ct = default)
    {
        var c = await db.Courses.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (c is null) return null;
        c.Name = req.Name.Trim();
        c.Branch = req.Branch;
        if (Enum.TryParse<EduLevel>(req.EduLevel, true, out var e)) c.EduLevel = e;
        c.Affiliation = req.Affiliation;
        c.SeatsAvailable = req.SeatsAvailable;
        c.Program = req.Program;
        c.ModifiedBy = user.UserId; c.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await courses.GetByIdAsync(id, ct);
    }

    public async Task<object?> CloneAsync(Guid id, string? newName, CancellationToken ct = default)
    {
        var src = await db.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (src is null) return null;
        var copy = new Course
        {
            OwnerId = user.OwnerId,
            Name = string.IsNullOrWhiteSpace(newName) ? src.Name + " (copy)" : newName!.Trim(),
            Branch = src.Branch, EduLevel = src.EduLevel, Affiliation = src.Affiliation,
            SeatsAvailable = src.SeatsAvailable, Program = src.Program, Rank = src.Rank,
            Terms = src.Terms.Select(t => new CourseTerm
            {
                Name = t.Name,
                Sections = t.Sections.Select(s => new CourseSection { Name = s.Name, Capacity = s.Capacity }).ToList(),
                Subjects = t.Subjects.Select(s => new CourseSubject { Name = s.Name, Code = s.Code, MaxMarks = s.MaxMarks, Optional = s.Optional }).ToList()
            }).ToList(),
            CreatedBy = user.UserId, CreatedOn = DateTime.UtcNow
        };
        db.Courses.Add(copy); await db.SaveChangesAsync(ct);
        return await courses.GetByIdAsync(copy.Id, ct);
    }

    public async Task<bool> RenameTermAsync(Guid id, string oldName, string newName, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        var t = c.Terms.FirstOrDefault(x => x.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase));
        if (t is null) return false;
        t.Name = newName; c.Terms = c.Terms.ToList(); Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> RemoveTermAsync(Guid id, string termName, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        c.Terms = c.Terms.Where(t => !t.Name.Equals(termName, StringComparison.OrdinalIgnoreCase)).ToList();
        Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> RenameSectionAsync(Guid id, string termName, string oldSection, string newSection, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        var t = c.Terms.FirstOrDefault(x => x.Name.Equals(termName, StringComparison.OrdinalIgnoreCase));
        var s = t?.Sections.FirstOrDefault(x => x.Name.Equals(oldSection, StringComparison.OrdinalIgnoreCase));
        if (s is null) return false;
        s.Name = newSection; c.Terms = c.Terms.ToList(); Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> RemoveSectionAsync(Guid id, string termName, string sectionName, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        var t = c.Terms.FirstOrDefault(x => x.Name.Equals(termName, StringComparison.OrdinalIgnoreCase));
        if (t is null) return false;
        t.Sections = t.Sections.Where(s => !s.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase)).ToList();
        c.Terms = c.Terms.ToList(); Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> RemoveSubjectAsync(Guid id, string termName, string subjectName, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        var t = c.Terms.FirstOrDefault(x => x.Name.Equals(termName, StringComparison.OrdinalIgnoreCase));
        if (t is null) return false;
        t.Subjects = t.Subjects.Where(s => !s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase)).ToList();
        c.Terms = c.Terms.ToList(); Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddOptionalSubjectAsync(Guid id, string termName, string subjectName, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        var t = c.Terms.FirstOrDefault(x => x.Name.Equals(termName, StringComparison.OrdinalIgnoreCase));
        if (t is null) { t = new CourseTerm { Name = termName }; c.Terms.Add(t); }
        if (!t.Subjects.Any(s => s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase)))
            t.Subjects.Add(new CourseSubject { Name = subjectName, Optional = true });
        c.Terms = c.Terms.ToList(); Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetRankAsync(Guid id, int rank, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        c.Rank = rank; Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetSortIndexAsync(Guid id, int sortIndex, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        c.SortIndex = sortIndex; Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetSeatsAsync(Guid id, int seats, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        c.SeatsAvailable = seats; Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public object GetFilters() => new
    {
        eduLevels = Enum.GetNames(typeof(EduLevel)),
        statuses = Enum.GetNames(typeof(CourseStatus))
    };

    public async Task<object> GetStudentsPlaceholderAsync(Guid courseId, CancellationToken ct = default)
        => new { courseId, students = Array.Empty<object>(), note = "Student roster is owned by Ct.Stu — join via course/section filters." };

    public async Task<object> FacetAsync(CancellationToken ct = default)
    {
        var courses = await db.Courses.AsNoTracking().Where(c => c.OwnerId == user.OwnerId && c.Status == CourseStatus.Active).ToListAsync(ct);
        return new
        {
            courses = courses.Select(c => new { c.Id, c.Name, c.Branch, sections = c.Terms.SelectMany(t => t.Sections.Select(s => s.Name)).Distinct() })
        };
    }

    private async Task<Course?> Load(Guid id, CancellationToken ct) =>
        await db.Courses.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
    private void Stamp(Course c) { c.ModifiedBy = user.UserId; c.ModifiedOn = DateTime.UtcNow; }
}

public sealed class AssessmentExtrasService(AcadDbContext db, IAcadUserContext user, AssessmentService assessments)
{
    public async Task<bool> UpdateSubjectAsync(Guid id, string subject, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Subject = subject; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }
    public async Task<bool> UpdateDurationAsync(Guid id, int minutes, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.DurationMinutes = minutes; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }
    public async Task<bool> UpdateTotalMarksAsync(Guid id, decimal marks, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.TotalMarks = marks; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }
    public async Task<bool> UnarchiveAsync(Guid id, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Status = AssessmentStatus.Draft; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }
    public async Task<object> SubmissionsCountAsync(Guid assessmentId, CancellationToken ct = default)
    {
        var count = await db.AssessmentSubmissions.CountAsync(s => s.AssessmentId == assessmentId, ct);
        return new { assessmentId, count };
    }
    public async Task<IReadOnlyList<object>> StudentSubmissionsAsync(string studentId, CancellationToken ct = default)
        => await db.AssessmentSubmissions.AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => new { s.Id, s.AssessmentId, s.SubmittedOn, s.Score, s.Evaluated })
            .ToListAsync(ct);

    public async Task<IReadOnlyList<object>> ByQuestionIdsAsync(List<Guid> questionIds, CancellationToken ct = default)
    {
        var ids = questionIds.Select(x => x.ToString()).ToHashSet();
        var list = await db.Assessments.AsNoTracking().Where(a => a.OwnerId == user.OwnerId).ToListAsync(ct);
        return list.Where(a => a.QuestionIds.Any(q => ids.Contains(q)))
            .Select(a => new { a.Id, a.Title, a.Status })
            .ToList();
    }

    public Task RefreshAsync(CancellationToken ct = default) => Task.CompletedTask; // no-op cache refresh

    private async Task<Assessment?> Load(Guid id, CancellationToken ct) =>
        await db.Assessments.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
    private void Stamp(Assessment a) { a.ModifiedBy = user.UserId; a.ModifiedOn = DateTime.UtcNow; }
}

public sealed class ExamExtrasService(AcadDbContext db, IAcadUserContext user, ExamService exams)
{
    public async Task<object?> GetStudentsAsync(Guid examId, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && x.OwnerId == user.OwnerId, ct);
        return e is null ? null : e.Students;
    }

    public async Task<object?> GetStudentScoresAsync(Guid examId, string studentId, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && x.OwnerId == user.OwnerId, ct);
        var s = e?.Students.FirstOrDefault(x => x.StudentId == studentId);
        return s is null ? null : new { s.StudentId, s.Name, s.Marks, Total = s.Marks.Sum(m => m.Marks ?? 0) };
    }

    public async Task<object> GetStudentPerformanceWidgetAsync(string studentId, CancellationToken ct = default)
    {
        var exams = await db.Exams.AsNoTracking().Where(e => e.OwnerId == user.OwnerId).ToListAsync(ct);
        var rows = exams.Select(e =>
        {
            var s = e.Students.FirstOrDefault(x => x.StudentId == studentId);
            return s is null ? null : new { Exam = e.Name, Total = s.Marks.Sum(m => m.Marks ?? 0), Max = e.Subjects.Sum(x => x.MaxMarks) };
        }).Where(x => x is not null).ToList();
        return new { studentId, series = rows };
    }

    public async Task<object> GenerateMarksCardAsync(Guid examId, string studentId, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && x.OwnerId == user.OwnerId, ct)
            ?? throw new KeyNotFoundException("Exam not found");
        var s = e.Students.FirstOrDefault(x => x.StudentId == studentId)
            ?? throw new KeyNotFoundException("Student not on exam");
        var rows = string.Join("", e.Subjects.Select(sub =>
        {
            var m = s.Marks.FirstOrDefault(x => x.SubjectName == sub.Name);
            return $"<tr><td>{System.Net.WebUtility.HtmlEncode(sub.Name)}</td><td>{m?.Marks}</td><td>{sub.MaxMarks}</td><td>{m?.Grade}</td></tr>";
        }));
        var total = s.Marks.Sum(m => m.Marks ?? 0);
        var max = e.Subjects.Sum(x => x.MaxMarks);
        var html = $"""
            <html><body>
            <h2>Marks Card — {System.Net.WebUtility.HtmlEncode(e.Name)}</h2>
            <p>Student: {System.Net.WebUtility.HtmlEncode(s.Name ?? s.StudentId)}</p>
            <table border="1" cellpadding="4"><tr><th>Subject</th><th>Marks</th><th>Max</th><th>Grade</th></tr>
            {rows}
            <tr><th>Total</th><th>{total}</th><th>{max}</th><th></th></tr></table>
            <p>Attendance: {s.Attendance} | Days present: {s.DaysPresent}/{s.DaysWorked}</p>
            <p>{System.Net.WebUtility.HtmlEncode(s.Remarks)}</p>
            </body></html>
            """;
        return new { examId, studentId, html, total, max };
    }

    public Task<object> EnqueMarksCardJobAsync(Guid examId, string? studentId, CancellationToken ct = default)
        => Task.FromResult<object>(new { queued = true, examId, studentId, note = "Wire to RabbitMQ worker when enabled" });

    public async Task<object> FacetAsync(CancellationToken ct = default)
    {
        var exams = await db.Exams.AsNoTracking().Where(e => e.OwnerId == user.OwnerId).Select(e => new { e.Id, e.Name, e.CourseId, e.CourseName }).ToListAsync(ct);
        return new { exams };
    }

    public async Task<object> HallTicketAsync(Guid examId, string studentId, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && x.OwnerId == user.OwnerId, ct);
        if (e is null) return new { error = "not found" };
        var s = e.Students.FirstOrDefault(x => x.StudentId == studentId);
        return new
        {
            title = "Hall Ticket",
            exam = e.Name,
            studentId,
            name = s?.Name,
            conductedOn = e.ConductedOn,
            subjects = e.Subjects.Select(x => x.Name),
            html = $"<h2>Hall Ticket</h2><p>{System.Net.WebUtility.HtmlEncode(e.Name)}</p><p>{System.Net.WebUtility.HtmlEncode(s?.Name ?? studentId)}</p>"
        };
    }
}

public sealed class ReportExtrasService(AcadDbContext db, IAcadUserContext user, ExamService exams, AcadReportService reports, ExamExtrasService examExtras)
{
    public async Task<object> ExamListAsync(CancellationToken ct = default)
        => await exams.GetAllAsync(null, ct);

    public async Task<object> FailuresAsync(Guid examId, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && x.OwnerId == user.OwnerId, ct);
        if (e is null) return new { failures = Array.Empty<object>() };
        var fails = e.Students.SelectMany(s => s.Marks.Select(m => new { s, m }))
            .Where(x =>
            {
                var sub = e.Subjects.FirstOrDefault(s => s.Name == x.m.SubjectName);
                return sub is not null && x.m.Marks is not null && x.m.Marks < sub.MinMarks;
            })
            .Select(x => new { x.s.StudentId, x.s.Name, x.m.SubjectName, x.m.Marks })
            .ToList();
        return new { examId, failures = fails };
    }

    public async Task<object> SubjectAverageAsync(Guid examId, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && x.OwnerId == user.OwnerId, ct);
        if (e is null) return new { };
        var avgs = e.Subjects.Select(sub =>
        {
            var marks = e.Students.Select(s => s.Marks.FirstOrDefault(m => m.SubjectName == sub.Name)?.Marks)
                .Where(m => m is not null).Select(m => m!.Value).ToList();
            return new { Subject = sub.Name, Average = marks.Count == 0 ? 0 : marks.Average(), Count = marks.Count };
        });
        return new { examId, subjects = avgs };
    }

    public async Task<object> GradewiseStatisticsAsync(Guid examId, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && x.OwnerId == user.OwnerId, ct);
        if (e is null) return new { };
        var grades = e.Students.SelectMany(s => s.Marks).Where(m => !string.IsNullOrEmpty(m.Grade))
            .GroupBy(m => m.Grade!).Select(g => new { Grade = g.Key, Count = g.Count() }).ToList();
        return new { examId, grades };
    }

    public async Task<object> FullMarksAsync(Guid examId, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && x.OwnerId == user.OwnerId, ct);
        if (e is null) return new { };
        var full = e.Students.Where(s => e.Subjects.All(sub =>
        {
            var m = s.Marks.FirstOrDefault(x => x.SubjectName == sub.Name)?.Marks;
            return m is not null && m >= sub.MaxMarks;
        })).Select(s => new { s.StudentId, s.Name });
        return new { examId, students = full };
    }

    public async Task<object> StudentPerformanceAsync(string studentId, CancellationToken ct = default)
        => await examExtras.GetStudentPerformanceWidgetAsync(studentId, ct);

    public async Task<string> StudentPerformanceCsvAsync(string studentId, CancellationToken ct = default)
    {
        var data = await examExtras.GetStudentPerformanceWidgetAsync(studentId, ct);
        return "studentId,note\n" + studentId + ",see JSON studentPerformance";
    }

    public async Task<object> MarksCardAsync(Guid examId, string studentId, CancellationToken ct = default)
        => await examExtras.GenerateMarksCardAsync(examId, studentId, ct);

    public async Task<object> HallTicketAsync(Guid examId, string studentId, CancellationToken ct = default)
        => await examExtras.HallTicketAsync(examId, studentId, ct);

    public Task<object> NoDueCertificateAsync(string studentId, CancellationToken ct = default)
        => Task.FromResult<object>(new
        {
            studentId,
            html = $"<h2>No Due Certificate</h2><p>Student {System.Net.WebUtility.HtmlEncode(studentId)} has no dues recorded in Acad module.</p>"
        });
}

public sealed class ArtefactService
{
    public byte[] StudentAttendanceTemplateCsv()
    {
        var csv = "StudentId,Name,Course,Section,Date,Status\n";
        return Encoding.UTF8.GetBytes(csv);
    }
}

public sealed class CalendarExtrasService(AcadDbContext db, IAcadUserContext user, CalendarService calendar)
{
    public Task<object> GetCalendarEventsAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
        => calendar.GetAsync(from, to, ct).ContinueWith(t => (object)t.Result, ct);

    public async Task<object> GenerateCalendarAsync(DateTime from, DateTime to, List<DayOfWeek>? weeklyHolidays, CancellationToken ct = default)
    {
        weeklyHolidays ??= [DayOfWeek.Sunday];
        var created = 0;
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (weeklyHolidays.Contains(d.DayOfWeek))
            {
                await calendar.CreateAsync($"Holiday", d, "Holiday", true, d.DayOfWeek.ToString(), ct);
                created++;
            }
        }
        return new { created, from, to };
    }

    public async Task<bool> RemoveOccurrenceAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.CalendarEvents.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (e is null) return false;
        db.CalendarEvents.Remove(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<object> RemoveCalendarRulesAsync(CancellationToken ct = default)
    {
        var holidays = await db.CalendarEvents.Where(x => x.OwnerId == user.OwnerId && x.IsHoliday).ToListAsync(ct);
        db.CalendarEvents.RemoveRange(holidays); await db.SaveChangesAsync(ct);
        return new { removed = holidays.Count };
    }
}
