using Ct.Acad.Domain;
using Ct.Acad.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Acad.Application;

public sealed class CourseService(AcadDbContext db, IAcadUserContext user)
{
    public async Task<IReadOnlyList<CourseDto>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Courses.AsNoTracking().Where(c => c.OwnerId == user.OwnerId);
        if (activeOnly) q = q.Where(c => c.Status == CourseStatus.Active);
        var list = await q.OrderBy(c => c.SortIndex).ThenBy(c => c.Name).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<CourseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var c = await db.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return c is null ? null : Map(c);
    }

    public async Task<CourseDto> CreateAsync(CreateCourseRequest req, CancellationToken ct = default)
    {
        var c = new Course
        {
            OwnerId = user.OwnerId,
            Name = req.Name.Trim(),
            Branch = req.Branch,
            EduLevel = Enum.TryParse<EduLevel>(req.EduLevel, true, out var e) ? e : EduLevel.Unknown,
            Affiliation = req.Affiliation,
            SeatsAvailable = req.SeatsAvailable,
            Program = req.Program,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Courses.Add(c);
        await db.SaveChangesAsync(ct);
        return Map(c);
    }

    public async Task<bool> EnableAsync(Guid id, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        c.Enable(); Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> DisableAsync(Guid id, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        c.Disable(); Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateNameAsync(Guid id, string name, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        c.Name = name.Trim(); Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddTermAsync(Guid id, string termName, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        if (c.Terms.Any(t => t.Name.Equals(termName, StringComparison.OrdinalIgnoreCase))) return true;
        c.Terms.Add(new CourseTerm { Name = termName });
        c.Terms = c.Terms.ToList();
        Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddSectionAsync(Guid id, string termName, string sectionName, int capacity, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        var term = c.Terms.FirstOrDefault(t => t.Name.Equals(termName, StringComparison.OrdinalIgnoreCase));
        if (term is null) { term = new CourseTerm { Name = termName }; c.Terms.Add(term); }
        if (!term.Sections.Any(s => s.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase)))
            term.Sections.Add(new CourseSection { Name = sectionName, Capacity = capacity });
        c.Terms = c.Terms.ToList();
        Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddSubjectAsync(Guid id, string termName, string subjectName, string? code, decimal? maxMarks, CancellationToken ct = default)
    {
        var c = await Load(id, ct); if (c is null) return false;
        var term = c.Terms.FirstOrDefault(t => t.Name.Equals(termName, StringComparison.OrdinalIgnoreCase));
        if (term is null) { term = new CourseTerm { Name = termName }; c.Terms.Add(term); }
        if (!term.Subjects.Any(s => s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase)))
            term.Subjects.Add(new CourseSubject { Name = subjectName, Code = code, MaxMarks = maxMarks });
        c.Terms = c.Terms.ToList();
        Stamp(c); await db.SaveChangesAsync(ct); return true;
    }

    private async Task<Course?> Load(Guid id, CancellationToken ct) =>
        await db.Courses.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
    private void Stamp(Course c) { c.ModifiedBy = user.UserId; c.ModifiedOn = DateTime.UtcNow; }

    private static CourseDto Map(Course c) => new()
    {
        Id = c.Id, Name = c.Name, Branch = c.Branch, EduLevel = c.EduLevel.ToString(),
        Status = c.Status.ToString(), SeatsAvailable = c.SeatsAvailable, Rank = c.Rank, Program = c.Program,
        Terms = c.Terms.Select(t => new TermDto
        {
            Name = t.Name,
            Sections = t.Sections.Select(s => s.Name).ToList(),
            Subjects = t.Subjects.Select(s => new SubjectDto { Name = s.Name, Code = s.Code, MaxMarks = s.MaxMarks }).ToList()
        }).ToList()
    };
}

public sealed class ExamService(AcadDbContext db, IAcadUserContext user)
{
    public async Task<IReadOnlyList<ExamDto>> GetAllAsync(string? courseId = null, CancellationToken ct = default)
    {
        var q = db.Exams.AsNoTracking().Where(e => e.OwnerId == user.OwnerId);
        if (!string.IsNullOrWhiteSpace(courseId)) q = q.Where(e => e.CourseId == courseId);
        var list = await q.OrderByDescending(e => e.CreatedOn).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<ExamDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return e is null ? null : Map(e);
    }

    public async Task<ExamDto> CreateAsync(CreateExamRequest req, CancellationToken ct = default)
    {
        var e = new Exam
        {
            OwnerId = user.OwnerId, Name = req.Name.Trim(), CourseId = req.CourseId,
            CourseName = req.CourseName, TermName = req.TermName,
            CreatedBy = user.UserId, CreatedOn = DateTime.UtcNow
        };
        db.Exams.Add(e); await db.SaveChangesAsync(ct); return Map(e);
    }

    public async Task<bool> EnableAsync(Guid id, CancellationToken ct = default)
    { var e = await Load(id, ct); if (e is null) return false; e.Enable(); Stamp(e); await db.SaveChangesAsync(ct); return true; }

    public async Task<bool> DisableAsync(Guid id, CancellationToken ct = default)
    { var e = await Load(id, ct); if (e is null) return false; e.Disable(); Stamp(e); await db.SaveChangesAsync(ct); return true; }

    public async Task<bool> LockAsync(Guid id, CancellationToken ct = default)
    { var e = await Load(id, ct); if (e is null) return false; e.Lock(); Stamp(e); await db.SaveChangesAsync(ct); return true; }

    public async Task<bool> UnlockAsync(Guid id, CancellationToken ct = default)
    { var e = await Load(id, ct); if (e is null) return false; e.Unlock(); Stamp(e); await db.SaveChangesAsync(ct); return true; }

    public async Task<bool> RenameAsync(Guid id, string name, CancellationToken ct = default)
    { var e = await Load(id, ct); if (e is null) return false; e.Name = name.Trim(); Stamp(e); await db.SaveChangesAsync(ct); return true; }

    public async Task<bool> SetConductedOnAsync(Guid id, DateTime date, CancellationToken ct = default)
    { var e = await Load(id, ct); if (e is null) return false; e.ConductedOn = date; Stamp(e); await db.SaveChangesAsync(ct); return true; }

    public async Task<bool> SetResultDateAsync(Guid id, DateTime date, CancellationToken ct = default)
    { var e = await Load(id, ct); if (e is null) return false; e.ResultDate = date; Stamp(e); await db.SaveChangesAsync(ct); return true; }

    public async Task<bool> AddSubjectAsync(Guid id, string subjectName, decimal maxMarks, decimal minMarks, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        if (e.Locked) throw new InvalidOperationException("Exam is locked.");
        if (!e.Subjects.Any(s => s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase)))
            e.Subjects.Add(new ExamSubject { Name = subjectName, MaxMarks = maxMarks, MinMarks = minMarks });
        e.Subjects = e.Subjects.ToList(); Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetStudentMarksAsync(Guid id, string studentId, string subjectName, decimal? marks, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        if (e.Locked) throw new InvalidOperationException("Exam is locked.");
        var stu = e.Students.FirstOrDefault(s => s.StudentId == studentId);
        if (stu is null) { stu = new ExamStudent { StudentId = studentId }; e.Students.Add(stu); }
        var m = stu.Marks.FirstOrDefault(x => x.SubjectName.Equals(subjectName, StringComparison.OrdinalIgnoreCase));
        if (m is null) { m = new ExamMark { SubjectName = subjectName }; stu.Marks.Add(m); }
        m.Marks = marks;
        e.Students = e.Students.ToList(); Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<object?> GetContentsAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return e is null ? null : new { e.Id, e.Name, e.Subjects, e.Students };
    }


    public async Task<bool> SetDaysPresentAsync(Guid id, string studentId, int daysPresent, int? daysWorked, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        var stu = e.Students.FirstOrDefault(s => s.StudentId == studentId);
        if (stu is null) { stu = new ExamStudent { StudentId = studentId }; e.Students.Add(stu); }
        stu.DaysPresent = daysPresent;
        if (daysWorked is not null) stu.DaysWorked = daysWorked;
        e.Students = e.Students.ToList(); Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetRemarksAsync(Guid id, string studentId, string remarks, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        var stu = e.Students.FirstOrDefault(s => s.StudentId == studentId);
        if (stu is null) { stu = new ExamStudent { StudentId = studentId }; e.Students.Add(stu); }
        stu.Remarks = remarks;
        e.Students = e.Students.ToList(); Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetAttendanceAsync(Guid id, string studentId, string attendance, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        var stu = e.Students.FirstOrDefault(s => s.StudentId == studentId);
        if (stu is null) { stu = new ExamStudent { StudentId = studentId }; e.Students.Add(stu); }
        stu.Attendance = attendance;
        e.Students = e.Students.ToList(); Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetMinMarksAsync(Guid id, string subjectName, decimal minMarks, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        var sub = e.Subjects.FirstOrDefault(s => s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase));
        if (sub is null) return false;
        sub.MinMarks = minMarks;
        e.Subjects = e.Subjects.ToList(); Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetSubjectGradingAsync(Guid id, string subjectName, string gradingId, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        var sub = e.Subjects.FirstOrDefault(s => s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase));
        if (sub is null) return false;
        sub.GradingId = gradingId;
        e.Subjects = e.Subjects.ToList(); Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<ExamDto?> CopyContentsAsync(Guid sourceId, string newName, CancellationToken ct = default)
    {
        var src = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sourceId && x.OwnerId == user.OwnerId, ct);
        if (src is null) return null;
        var copy = new Exam
        {
            OwnerId = user.OwnerId,
            Name = string.IsNullOrWhiteSpace(newName) ? src.Name + " (copy)" : newName.Trim(),
            CourseId = src.CourseId,
            CourseName = src.CourseName,
            TermName = src.TermName,
            Subjects = src.Subjects.Select(s => new ExamSubject { Name = s.Name, MaxMarks = s.MaxMarks, MinMarks = s.MinMarks, GradingId = s.GradingId }).ToList(),
            Students = [],
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Exams.Add(copy); await db.SaveChangesAsync(ct); return Map(copy);
    }

    public async Task<bool> RemoveSubjectAsync(Guid id, string subjectName, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        if (e.Locked) throw new InvalidOperationException("Exam is locked.");
        e.Subjects = e.Subjects.Where(s => !s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase)).ToList();
        Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetEvaluatedOnAsync(Guid id, DateTime date, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        e.EvaluatedOn = date; Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddStudentAsync(Guid id, string studentId, string? name, CancellationToken ct = default)
    {
        var e = await Load(id, ct); if (e is null) return false;
        if (!e.Students.Any(s => s.StudentId == studentId))
            e.Students.Add(new ExamStudent { StudentId = studentId, Name = name });
        e.Students = e.Students.ToList(); Stamp(e); await db.SaveChangesAsync(ct); return true;
    }

    private async Task<Exam?> Load(Guid id, CancellationToken ct) =>
        await db.Exams.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
    private void Stamp(Exam e) { e.ModifiedBy = user.UserId; e.ModifiedOn = DateTime.UtcNow; }
    private static ExamDto Map(Exam e) => new()
    {
        Id = e.Id, Name = e.Name, CourseId = e.CourseId, CourseName = e.CourseName, TermName = e.TermName,
        Status = e.Status.ToString(), Locked = e.Locked, ConductedOn = e.ConductedOn, ResultDate = e.ResultDate,
        SubjectCount = e.Subjects.Count, StudentCount = e.Students.Count
    };
}

public sealed class AssessmentService(AcadDbContext db, IAcadUserContext user)
{
    public async Task<IReadOnlyList<AssessmentDto>> GetAllAsync(bool publishedOnly = false, CancellationToken ct = default)
    {
        var q = db.Assessments.AsNoTracking().Where(a => a.OwnerId == user.OwnerId);
        if (publishedOnly) q = q.Where(a => a.Status == AssessmentStatus.Published);
        return (await q.OrderByDescending(a => a.CreatedOn).ToListAsync(ct)).Select(Map).ToList();
    }

    public async Task<AssessmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var a = await db.Assessments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return a is null ? null : Map(a);
    }

    public async Task<AssessmentDto> CreateAsync(CreateAssessmentRequest req, CancellationToken ct = default)
    {
        var a = new Assessment
        {
            OwnerId = user.OwnerId, Title = req.Title.Trim(), Subject = req.Subject,
            DurationMinutes = req.DurationMinutes, TotalMarks = req.TotalMarks,
            QuestionIds = req.QuestionIds ?? [], Tags = req.Tags ?? [],
            CreatedBy = user.UserId, CreatedOn = DateTime.UtcNow
        };
        db.Assessments.Add(a); await db.SaveChangesAsync(ct); return Map(a);
    }

    public async Task<bool> UpdateAsync(Guid id, CreateAssessmentRequest req, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Title = req.Title.Trim(); a.Subject = req.Subject; a.DurationMinutes = req.DurationMinutes;
        a.TotalMarks = req.TotalMarks;
        if (req.QuestionIds is not null) a.QuestionIds = req.QuestionIds;
        if (req.Tags is not null) a.Tags = req.Tags;
        Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> PublishAsync(Guid id, CancellationToken ct = default)
    { var a = await Load(id, ct); if (a is null) return false; a.Publish(); Stamp(a); await db.SaveChangesAsync(ct); return true; }
    public async Task<bool> UnpublishAsync(Guid id, CancellationToken ct = default)
    { var a = await Load(id, ct); if (a is null) return false; a.Unpublish(); Stamp(a); await db.SaveChangesAsync(ct); return true; }
    public async Task<bool> ArchiveAsync(Guid id, CancellationToken ct = default)
    { var a = await Load(id, ct); if (a is null) return false; a.Archive(); Stamp(a); await db.SaveChangesAsync(ct); return true; }

    public async Task<Guid> SubmitAsync(Guid assessmentId, string studentId, string? studentName, Dictionary<string, string> answers, CancellationToken ct = default)
    {
        var a = await db.Assessments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == assessmentId && x.OwnerId == user.OwnerId, ct)
            ?? throw new KeyNotFoundException("Assessment not found");
        if (a.Status != AssessmentStatus.Published) throw new InvalidOperationException("Assessment not published");
        var sub = new AssessmentSubmission
        {
            AssessmentId = assessmentId, StudentId = studentId, StudentName = studentName, Answers = answers
        };
        db.AssessmentSubmissions.Add(sub); await db.SaveChangesAsync(ct); return sub.Id;
    }

    public async Task<IReadOnlyList<object>> GetSubmissionsAsync(Guid assessmentId, CancellationToken ct = default)
    {
        return await db.AssessmentSubmissions.AsNoTracking()
            .Where(s => s.AssessmentId == assessmentId)
            .Select(s => new { s.Id, s.StudentId, s.StudentName, s.SubmittedOn, s.Score, s.Evaluated })
            .ToListAsync(ct);
    }

    public async Task<bool> EvaluateAsync(Guid submissionId, decimal score, CancellationToken ct = default)
    {
        var s = await db.AssessmentSubmissions.FirstOrDefaultAsync(x => x.Id == submissionId, ct);
        if (s is null) return false;
        s.Score = score; s.Evaluated = true; await db.SaveChangesAsync(ct); return true;
    }

    // Tags
    public async Task<IReadOnlyList<object>> GetTagsAsync(CancellationToken ct = default) =>
        await db.Tags.AsNoTracking().Where(t => t.OwnerId == user.OwnerId)
            .Select(t => new { t.Id, t.Name, t.Enabled }).ToListAsync(ct);

    public async Task<object> CreateTagAsync(string name, CancellationToken ct = default)
    {
        var tag = new AcadTag { OwnerId = user.OwnerId, Name = name.Trim() };
        db.Tags.Add(tag); await db.SaveChangesAsync(ct);
        return new { tag.Id, tag.Name };
    }

    public async Task<bool> SetTagEnabledAsync(Guid id, bool enabled, CancellationToken ct = default)
    {
        var t = await db.Tags.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (t is null) return false;
        t.Enabled = enabled; await db.SaveChangesAsync(ct); return true;
    }

    private async Task<Assessment?> Load(Guid id, CancellationToken ct) =>
        await db.Assessments.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
    private void Stamp(Assessment a) { a.ModifiedBy = user.UserId; a.ModifiedOn = DateTime.UtcNow; }
    private static AssessmentDto Map(Assessment a) => new()
    {
        Id = a.Id, Title = a.Title, Subject = a.Subject, DurationMinutes = a.DurationMinutes,
        TotalMarks = a.TotalMarks, Status = a.Status.ToString(), Tags = a.Tags, QuestionCount = a.QuestionIds.Count
    };
}

public sealed class AttendanceService(AcadDbContext db, IAcadUserContext user)
{
    public async Task<AttendanceDto> PostAsync(PostAttendanceRequest req, CancellationToken ct = default)
    {
        var ev = new AttendanceEvent
        {
            OwnerId = user.OwnerId, Date = req.Date.Date, CourseId = req.CourseId,
            CourseName = req.CourseName, Section = req.Section,
            AbsenteeStudentIds = req.AbsenteeStudentIds.Distinct().ToList(),
            CreatedBy = user.UserId, CreatedOn = DateTime.UtcNow
        };
        db.AttendanceEvents.Add(ev); await db.SaveChangesAsync(ct);
        return Map(ev);
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetAbsenteesAsync(DateTime? on, string? courseId, CancellationToken ct = default)
    {
        var q = db.AttendanceEvents.AsNoTracking().Where(e => e.OwnerId == user.OwnerId);
        if (on is not null) q = q.Where(e => e.Date == on.Value.Date);
        if (!string.IsNullOrWhiteSpace(courseId)) q = q.Where(e => e.CourseId == courseId);
        return (await q.OrderByDescending(e => e.Date).Take(100).ToListAsync(ct)).Select(Map).ToList();
    }

    private static AttendanceDto Map(AttendanceEvent e) => new()
    {
        Id = e.Id, Date = e.Date, CourseId = e.CourseId, CourseName = e.CourseName, Section = e.Section,
        AbsenteeStudentIds = e.AbsenteeStudentIds, AbsenteeCount = e.AbsenteeStudentIds.Count
    };
}

public sealed class GradingService(AcadDbContext db, IAcadUserContext user)
{
    public async Task<IReadOnlyList<GradingDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await db.GradingSchemes.AsNoTracking().Where(g => g.OwnerId == user.OwnerId).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<GradingDto> CreateAsync(string name, List<GradingRuleDto> rules, CancellationToken ct = default)
    {
        var g = new GradingScheme
        {
            OwnerId = user.OwnerId, Name = name.Trim(), CreatedBy = user.UserId, CreatedOn = DateTime.UtcNow,
            Rules = rules.Select(r => new GradingRule { Grade = r.Grade, MinPercent = r.MinPercent, MaxPercent = r.MaxPercent }).ToList()
        };
        db.GradingSchemes.Add(g); await db.SaveChangesAsync(ct); return Map(g);
    }

    public async Task<bool> EnableAsync(Guid id, CancellationToken ct = default)
    { var g = await Load(id, ct); if (g is null) return false; g.Enable(); await db.SaveChangesAsync(ct); return true; }
    public async Task<bool> DisableAsync(Guid id, CancellationToken ct = default)
    { var g = await Load(id, ct); if (g is null) return false; g.Disable(); await db.SaveChangesAsync(ct); return true; }

    public async Task<string> GetGradeAsync(Guid id, decimal marks, decimal totalMarks, CancellationToken ct = default)
    {
        var g = await db.GradingSchemes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (g is null || totalMarks <= 0) return "";
        return g.GetGrade(marks * 100 / totalMarks);
    }

    private async Task<GradingScheme?> Load(Guid id, CancellationToken ct) =>
        await db.GradingSchemes.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
    private static GradingDto Map(GradingScheme g) => new()
    {
        Id = g.Id, Name = g.Name, Status = g.Status.ToString(),
        Rules = g.Rules.Select(r => new GradingRuleDto { Grade = r.Grade, MinPercent = r.MinPercent, MaxPercent = r.MaxPercent }).ToList()
    };
}
