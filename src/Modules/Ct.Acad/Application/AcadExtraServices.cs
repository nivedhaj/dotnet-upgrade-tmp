using Ct.Acad.Domain;
using Ct.Acad.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Acad.Application;

public sealed class LmsService(AcadDbContext db, IAcadUserContext user)
{
    public async Task<object> CreateQuestionAsync(CreateQuestionRequest req, CancellationToken ct = default)
    {
        var q = new Question
        {
            OwnerId = user.OwnerId,
            Text = req.Text.Trim(),
            Type = req.Type ?? "MCQ",
            Options = req.Options ?? [],
            Answer = req.Answer,
            Explanation = req.Explanation,
            Tags = req.Tags ?? [],
            Subject = req.Subject,
            Marks = req.Marks <= 0 ? 1 : req.Marks,
            ImageUrl = req.ImageUrl,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Questions.Add(q);
        await db.SaveChangesAsync(ct);
        return Map(q);
    }

    public async Task<IReadOnlyList<object>> GetQuestionsAsync(string? tag = null, string? subject = null, bool publishedOnly = false, CancellationToken ct = default)
    {
        var q = db.Questions.AsNoTracking().Where(x => x.OwnerId == user.OwnerId);
        if (publishedOnly) q = q.Where(x => x.Status == QuestionStatus.Published);
        if (!string.IsNullOrWhiteSpace(subject)) q = q.Where(x => x.Subject == subject);
        var list = await q.OrderByDescending(x => x.CreatedOn).Take(500).ToListAsync(ct);
        if (!string.IsNullOrWhiteSpace(tag))
            list = list.Where(x => x.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase))).ToList();
        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<object>> GetRandomAsync(int count = 10, string? tag = null, CancellationToken ct = default)
    {
        var all = await GetQuestionsAsync(tag, null, true, ct);
        return all.OrderBy(_ => Guid.NewGuid()).Take(Math.Clamp(count, 1, 50)).ToList();
    }

    public async Task<object?> SubmitAnswerAsync(Guid questionId, string answer, CancellationToken ct = default)
    {
        var q = await db.Questions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == questionId && x.OwnerId == user.OwnerId, ct);
        if (q is null) return null;
        var correct = string.Equals(q.Answer?.Trim(), answer?.Trim(), StringComparison.OrdinalIgnoreCase);
        return new { questionId, correct, expected = q.Answer, explanation = q.Explanation };
    }

    public async Task<bool> UpdateQuestionAsync(Guid id, CreateQuestionRequest req, CancellationToken ct = default)
    {
        var q = await Load(id, ct); if (q is null) return false;
        q.Text = req.Text.Trim();
        if (req.Type is not null) q.Type = req.Type;
        if (req.Options is not null) q.Options = req.Options;
        if (req.Answer is not null) q.Answer = req.Answer;
        if (req.Explanation is not null) q.Explanation = req.Explanation;
        if (req.Tags is not null) q.Tags = req.Tags;
        if (req.Subject is not null) q.Subject = req.Subject;
        if (req.Marks > 0) q.Marks = req.Marks;
        if (req.ImageUrl is not null) q.ImageUrl = req.ImageUrl;
        q.ModifiedBy = user.UserId; q.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetStatusAsync(Guid id, QuestionStatus status, CancellationToken ct = default)
    {
        var q = await Load(id, ct); if (q is null) return false;
        q.Status = status; q.ModifiedBy = user.UserId; q.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateQuestionsTagsAsync(List<Guid> ids, List<string> tags, CancellationToken ct = default)
    {
        var list = await db.Questions.Where(x => x.OwnerId == user.OwnerId && ids.Contains(x.Id)).ToListAsync(ct);
        foreach (var q in list) { q.Tags = tags; q.ModifiedOn = DateTime.UtcNow; }
        await db.SaveChangesAsync(ct); return list.Count > 0;
    }

    public async Task<object> CreateImageTagAsync(string name, string? imageUrl, CancellationToken ct = default)
    {
        var tag = new AcadTag { OwnerId = user.OwnerId, Name = $"img:{name.Trim()}" };
        db.Tags.Add(tag); await db.SaveChangesAsync(ct);
        return new { tag.Id, tag.Name, imageUrl };
    }

    private async Task<Question?> Load(Guid id, CancellationToken ct) =>
        await db.Questions.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);

    private static object Map(Question q) => new
    {
        q.Id, q.Text, q.Type, q.Options, q.Answer, q.Explanation, q.Tags, q.Subject, q.Marks,
        Status = q.Status.ToString(), q.ImageUrl, q.CreatedOn
    };
}

public sealed class CreateQuestionRequest
{
    public required string Text { get; set; }
    public string? Type { get; set; }
    public List<string>? Options { get; set; }
    public string? Answer { get; set; }
    public string? Explanation { get; set; }
    public List<string>? Tags { get; set; }
    public string? Subject { get; set; }
    public decimal Marks { get; set; } = 1;
    public string? ImageUrl { get; set; }
}

public sealed class TtService(AcadDbContext db, IAcadUserContext user)
{
    public async Task<IReadOnlyList<object>> GetAsync(string? courseId = null, CancellationToken ct = default)
    {
        var q = db.TimeTables.AsNoTracking().Where(t => t.OwnerId == user.OwnerId && t.Status != "Cancelled");
        if (!string.IsNullOrWhiteSpace(courseId)) q = q.Where(t => t.CourseId == courseId);
        var list = await q.OrderByDescending(t => t.CreatedOn).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<object> SetRulesAsync(TtRulesRequest req, CancellationToken ct = default)
    {
        var tt = await FindOrCreateAsync(req.CourseId, req.CourseName, req.Section, req.TermName, ct);
        tt.Rules = req.Rules ?? new Dictionary<string, object>();
        tt.ModifiedBy = user.UserId; tt.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(tt);
    }

    public async Task<object> GenerateAsync(TtGenerateRequest req, CancellationToken ct = default)
    {
        var tt = await FindOrCreateAsync(req.CourseId, req.CourseName, req.Section, req.TermName, ct);
        tt.Status = "Generating";
        // Simple deterministic grid: Mon–Fri × period count
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        var periods = new List<TtPeriod>();
        var periodCount = Math.Clamp(req.PeriodCount <= 0 ? 6 : req.PeriodCount, 1, 12);
        var subjects = req.Subjects ?? [];
        var staff = req.StaffIds ?? [];
        var si = 0;
        foreach (var day in days)
        {
            for (var n = 1; n <= periodCount; n++)
            {
                var isBreak = n == periodCount / 2 + 1;
                periods.Add(new TtPeriod
                {
                    Number = n,
                    DayOfWeek = day,
                    FromTime = $"{7 + n:D2}:00",
                    ToTime = $"{7 + n:D2}:45",
                    Break = isBreak,
                    BreakDescription = isBreak ? "Break" : null,
                    SubjectName = isBreak || subjects.Count == 0 ? null : subjects[si % subjects.Count],
                    StaffId = isBreak || staff.Count == 0 ? null : staff[si % staff.Count]
                });
                if (!isBreak) si++;
            }
        }
        tt.Periods = periods;
        tt.Status = "Active";
        tt.ModifiedBy = user.UserId; tt.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(tt);
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var tt = await db.TimeTables.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (tt is null) return false;
        tt.Status = "Cancelled";
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<object?> PrintAsync(Guid id, CancellationToken ct = default)
    {
        var tt = await db.TimeTables.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (tt is null) return null;
        var rows = tt.Periods.GroupBy(p => p.Number).Select(g => new
        {
            Period = g.Key,
            Cells = g.Select(c => new { c.DayOfWeek, c.SubjectName, c.StaffId, c.Break, c.FromTime, c.ToTime })
        });
        return new { tt.Id, tt.CourseName, tt.Section, Grid = rows, Html = RenderHtml(tt) };
    }

    private async Task<TimeTable> FindOrCreateAsync(string? courseId, string? courseName, string? section, string? term, CancellationToken ct)
    {
        TimeTable? tt = null;
        if (!string.IsNullOrWhiteSpace(courseId))
            tt = await db.TimeTables.FirstOrDefaultAsync(x => x.OwnerId == user.OwnerId && x.CourseId == courseId && x.Section == section && x.Status != "Cancelled", ct);
        if (tt is null)
        {
            tt = new TimeTable
            {
                OwnerId = user.OwnerId, CourseId = courseId, CourseName = courseName,
                Section = section, TermName = term, CreatedBy = user.UserId, CreatedOn = DateTime.UtcNow
            };
            db.TimeTables.Add(tt);
        }
        return tt;
    }

    private static object Map(TimeTable t) => new
    {
        t.Id, t.CourseId, t.CourseName, t.Section, t.TermName, t.Status,
        PeriodCount = t.Periods.Count, t.CreatedOn
    };

    private static string RenderHtml(TimeTable t)
    {
        var days = t.Periods.Select(p => p.DayOfWeek).Distinct().ToList();
        var nums = t.Periods.Select(p => p.Number).Distinct().OrderBy(n => n).ToList();
        var sb = new System.Text.StringBuilder();
        sb.Append($"<h3>Timetable {System.Net.WebUtility.HtmlEncode(t.CourseName)} {System.Net.WebUtility.HtmlEncode(t.Section)}</h3><table border='1' cellpadding='4'><tr><th>Period</th>");
        foreach (var d in days) sb.Append($"<th>{d}</th>");
        sb.Append("</tr>");
        foreach (var n in nums)
        {
            sb.Append($"<tr><td>{n}</td>");
            foreach (var d in days)
            {
                var cell = t.Periods.FirstOrDefault(p => p.Number == n && p.DayOfWeek == d);
                sb.Append($"<td>{(cell?.Break == true ? "Break" : System.Net.WebUtility.HtmlEncode(cell?.SubjectName ?? ""))}</td>");
            }
            sb.Append("</tr>");
        }
        sb.Append("</table>");
        return sb.ToString();
    }
}

public sealed class TtRulesRequest
{
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Section { get; set; }
    public string? TermName { get; set; }
    public Dictionary<string, object>? Rules { get; set; }
}

public sealed class TtGenerateRequest
{
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Section { get; set; }
    public string? TermName { get; set; }
    public int PeriodCount { get; set; } = 6;
    public List<string>? Subjects { get; set; }
    public List<string>? StaffIds { get; set; }
}

public sealed class CalendarService(AcadDbContext db, IAcadUserContext user)
{
    public async Task<IReadOnlyList<object>> GetAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var q = db.CalendarEvents.AsNoTracking().Where(e => e.OwnerId == user.OwnerId);
        if (from is not null) q = q.Where(e => e.Date >= from.Value.Date);
        if (to is not null) q = q.Where(e => e.Date <= to.Value.Date);
        return await q.OrderBy(e => e.Date).Select(e => new
        {
            e.Id, e.Title, e.Date, e.EndDate, e.Type, e.Description, e.IsHoliday
        }).ToListAsync(ct);
    }

    public async Task<object> CreateAsync(string title, DateTime date, string? type, bool isHoliday, string? description, CancellationToken ct = default)
    {
        var e = new CalendarEvent
        {
            OwnerId = user.OwnerId, Title = title, Date = date.Date, Type = type,
            IsHoliday = isHoliday, Description = description
        };
        db.CalendarEvents.Add(e); await db.SaveChangesAsync(ct);
        return new { e.Id, e.Title, e.Date };
    }

    public async Task<IReadOnlyList<object>> HolidaysAsync(CancellationToken ct = default) =>
        await db.CalendarEvents.AsNoTracking()
            .Where(e => e.OwnerId == user.OwnerId && e.IsHoliday)
            .OrderBy(e => e.Date)
            .Select(e => new { e.Id, e.Title, e.Date, e.Description })
            .ToListAsync(ct);

    public async Task<IReadOnlyList<object>> UpcomingAsync(int days = 30, CancellationToken ct = default)
    {
        var until = DateTime.UtcNow.Date.AddDays(days);
        return await db.CalendarEvents.AsNoTracking()
            .Where(e => e.OwnerId == user.OwnerId && e.Date >= DateTime.UtcNow.Date && e.Date <= until)
            .OrderBy(e => e.Date)
            .Select(e => new { e.Id, e.Title, e.Date, e.Type, e.IsHoliday })
            .ToListAsync(ct);
    }
}

public sealed class AcadReportService(AcadDbContext db, IAcadUserContext user, ExamService exams, CourseService courses, AttendanceService attendance, CalendarService calendar)
{
    public async Task<object> CourseListAsync(CancellationToken ct = default)
        => await courses.GetAllAsync(true, ct);

    public async Task<object> ConsolidatedMarksAsync(Guid examId, CancellationToken ct = default)
    {
        var contents = await exams.GetContentsAsync(examId, ct);
        return contents ?? new { message = "Exam not found" };
    }

    public async Task<object> RankListAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId && e.OwnerId == user.OwnerId, ct);
        if (exam is null) return new { ranks = Array.Empty<object>() };
        var ranks = exam.Students
            .Select(s => new
            {
                s.StudentId, s.Name,
                Total = s.Marks.Sum(m => m.Marks ?? 0),
                Max = exam.Subjects.Sum(x => x.MaxMarks)
            })
            .OrderByDescending(x => x.Total)
            .Select((x, i) => new { Rank = i + 1, x.StudentId, x.Name, x.Total, x.Max })
            .ToList();
        return new { examId, exam.Name, ranks };
    }

    public async Task<object> TopTenAsync(Guid examId, CancellationToken ct = default)
    {
        var rank = await RankListAsync(examId, ct);
        return rank;
    }

    public async Task<object> SubjectTopScorersAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId && e.OwnerId == user.OwnerId, ct);
        if (exam is null) return new { subjects = Array.Empty<object>() };
        var result = exam.Subjects.Select(sub =>
        {
            var top = exam.Students
                .Select(s => new { s.StudentId, s.Name, Marks = s.Marks.FirstOrDefault(m => m.SubjectName == sub.Name)?.Marks })
                .Where(x => x.Marks is not null)
                .OrderByDescending(x => x.Marks)
                .Take(5)
                .ToList();
            return new { Subject = sub.Name, Top = top };
        });
        return new { examId, subjects = result };
    }

    public async Task<object> ResultAtAGlanceAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId && e.OwnerId == user.OwnerId, ct);
        if (exam is null) return new { };
        var totals = exam.Students.Select(s => s.Marks.Sum(m => m.Marks ?? 0)).ToList();
        return new
        {
            exam.Name,
            StudentCount = exam.Students.Count,
            SubjectCount = exam.Subjects.Count,
            AvgTotal = totals.Count == 0 ? 0 : totals.Average(),
            MaxTotal = totals.Count == 0 ? 0 : totals.Max(),
            MinTotal = totals.Count == 0 ? 0 : totals.Min()
        };
    }

    public async Task<object> ExamAttendanceAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId && e.OwnerId == user.OwnerId, ct);
        if (exam is null) return new { };
        return new
        {
            exam.Name,
            Present = exam.Students.Count(s => s.Attendance != "A"),
            Absent = exam.Students.Count(s => s.Attendance == "A"),
            Students = exam.Students.Select(s => new { s.StudentId, s.Name, s.Attendance, s.DaysPresent, s.DaysWorked })
        };
    }

    public Task<object> HolidayListAsync(CancellationToken ct = default)
        => calendar.HolidaysAsync(ct).ContinueWith(t => (object)t.Result, ct);

    public Task<object> UpcomingEventsAsync(CancellationToken ct = default)
        => calendar.UpcomingAsync(30, ct).ContinueWith(t => (object)t.Result, ct);

    public async Task<object> TodaysEventsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var events = await calendar.GetAsync(today, today, ct);
        var abs = await attendance.GetAbsenteesAsync(today, null, ct);
        return new { date = today, events, absentees = abs };
    }
}
