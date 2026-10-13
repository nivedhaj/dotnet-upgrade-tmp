namespace Ct.Acad.Domain;

public enum CourseStatus { Unknown = 0, Active = 1, Disabled = 99 }
public enum EduLevel { Unknown = 0, PrePrimary = 10, Primary = 20, Secondary = 30, HigherSecondary = 40, UG = 50, PG = 60 }
public enum ExamStatus { Unknown = 0, Active = 1, Locked = 50, Disabled = 99 }
public enum AssessmentStatus { Draft = 0, Published = 1, Archived = 90, Disabled = 99 }
public enum GradingStatus { Active = 1, Disabled = 99 }

public class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Branch { get; set; }
    public EduLevel EduLevel { get; set; } = EduLevel.Unknown;
    public string? Affiliation { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Active;
    public int SortIndex { get; set; } = -1;
    public int Rank { get; set; }
    public int SeatsAvailable { get; set; }
    public string? Program { get; set; }
    public List<CourseTerm> Terms { get; set; } = [];
    public List<string> ExamSubjectOrder { get; set; } = [];
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public void Enable() => Status = CourseStatus.Active;
    public void Disable() => Status = CourseStatus.Disabled;
}

public class CourseTerm
{
    public string Name { get; set; } = "";
    public List<CourseSection> Sections { get; set; } = [];
    public List<CourseSubject> Subjects { get; set; } = [];
}

public class CourseSection
{
    public string Name { get; set; } = "";
    public int Capacity { get; set; }
}

public class CourseSubject
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public decimal? MaxMarks { get; set; }
    public bool Optional { get; set; }
}

public class Exam
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? TermName { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.Active;
    public DateTime? ConductedOn { get; set; }
    public DateTime? ResultDate { get; set; }
    public DateTime? EvaluatedOn { get; set; }
    public bool Locked { get; set; }
    public List<ExamSubject> Subjects { get; set; } = [];
    public List<ExamStudent> Students { get; set; } = [];
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public void Enable() { Status = ExamStatus.Active; Locked = false; }
    public void Disable() => Status = ExamStatus.Disabled;
    public void Lock() { Locked = true; Status = ExamStatus.Locked; }
    public void Unlock() { Locked = false; if (Status == ExamStatus.Locked) Status = ExamStatus.Active; }
}

public class ExamSubject
{
    public string Name { get; set; } = "";
    public decimal MaxMarks { get; set; }
    public decimal MinMarks { get; set; }
    public string? GradingId { get; set; }
}

public class ExamStudent
{
    public string StudentId { get; set; } = "";
    public string? Name { get; set; }
    public int? DaysPresent { get; set; }
    public int? DaysWorked { get; set; }
    public string? Remarks { get; set; }
    public string? Attendance { get; set; }
    public List<ExamMark> Marks { get; set; } = [];
}

public class ExamMark
{
    public string SubjectName { get; set; } = "";
    public decimal? Marks { get; set; }
    public string? Grade { get; set; }
}

public class Assessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Subject { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalMarks { get; set; }
    public AssessmentStatus Status { get; set; } = AssessmentStatus.Draft;
    public List<string> QuestionIds { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public void Publish() => Status = AssessmentStatus.Published;
    public void Unpublish() => Status = AssessmentStatus.Draft;
    public void Archive() => Status = AssessmentStatus.Archived;
}

public class AssessmentSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssessmentId { get; set; }
    public string StudentId { get; set; } = "";
    public string? StudentName { get; set; }
    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    public decimal? Score { get; set; }
    public bool Evaluated { get; set; }
    public Dictionary<string, string> Answers { get; set; } = new();
}

public class AttendanceEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public DateTime Date { get; set; }
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Section { get; set; }
    public List<string> AbsenteeStudentIds { get; set; } = [];
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class GradingScheme
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Name { get; set; } = "";
    public GradingStatus Status { get; set; } = GradingStatus.Active;
    public List<GradingRule> Rules { get; set; } = [];
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public void Enable() => Status = GradingStatus.Active;
    public void Disable() => Status = GradingStatus.Disabled;

    public string GetGrade(decimal marksPercentage)
    {
        foreach (var rule in Rules.OrderByDescending(r => r.MinPercent))
            if (marksPercentage >= rule.MinPercent && marksPercentage <= rule.MaxPercent)
                return rule.Grade;
        return "";
    }
}

public class GradingRule
{
    public string Grade { get; set; } = "";
    public decimal MinPercent { get; set; }
    public decimal MaxPercent { get; set; }
}

public class AcadTag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Enabled { get; set; } = true;
}

public enum QuestionStatus { Draft = 0, Published = 1, Archived = 90, Disabled = 99 }

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Text { get; set; } = "";
    public string? Type { get; set; } // MCQ, Descriptive, etc.
    public List<string> Options { get; set; } = [];
    public string? Answer { get; set; }
    public string? Explanation { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? Subject { get; set; }
    public decimal Marks { get; set; } = 1;
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;
    public string? ImageUrl { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public void Publish() => Status = QuestionStatus.Published;
    public void Unpublish() => Status = QuestionStatus.Draft;
    public void Archive() => Status = QuestionStatus.Archived;
    public void Enable() => Status = QuestionStatus.Published;
    public void Disable() => Status = QuestionStatus.Disabled;
}

public class TimeTable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Section { get; set; }
    public string? TermName { get; set; }
    public string Status { get; set; } = "Active"; // Active, Cancelled, Generating
    public List<TtPeriod> Periods { get; set; } = [];
    public Dictionary<string, object>? Rules { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class TtPeriod
{
    public int Number { get; set; }
    public string DayOfWeek { get; set; } = "Monday";
    public string? FromTime { get; set; }
    public string? ToTime { get; set; }
    public bool Break { get; set; }
    public string? BreakDescription { get; set; }
    public string? SubjectName { get; set; }
    public string? StaffId { get; set; }
    public string? StaffName { get; set; }
}

public class CalendarEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Type { get; set; } // Holiday, Event, Exam
    public string? Description { get; set; }
    public bool IsHoliday { get; set; }
}
