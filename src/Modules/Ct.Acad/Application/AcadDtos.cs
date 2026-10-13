namespace Ct.Acad.Application;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public sealed class CourseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Branch { get; set; }
    public string EduLevel { get; set; } = "";
    public string Status { get; set; } = "";
    public int SeatsAvailable { get; set; }
    public int Rank { get; set; }
    public string? Program { get; set; }
    public List<TermDto> Terms { get; set; } = [];
}

public sealed class TermDto
{
    public string Name { get; set; } = "";
    public List<string> Sections { get; set; } = [];
    public List<SubjectDto> Subjects { get; set; } = [];
}

public sealed class SubjectDto
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public decimal? MaxMarks { get; set; }
}

public sealed class CreateCourseRequest
{
    public required string Name { get; set; }
    public string? Branch { get; set; }
    public string EduLevel { get; set; } = "Unknown";
    public string? Affiliation { get; set; }
    public int SeatsAvailable { get; set; }
    public string? Program { get; set; }
}

public sealed class ExamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? TermName { get; set; }
    public string Status { get; set; } = "";
    public bool Locked { get; set; }
    public DateTime? ConductedOn { get; set; }
    public DateTime? ResultDate { get; set; }
    public int SubjectCount { get; set; }
    public int StudentCount { get; set; }
}

public sealed class CreateExamRequest
{
    public required string Name { get; set; }
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? TermName { get; set; }
}

public sealed class AssessmentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Subject { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalMarks { get; set; }
    public string Status { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public int QuestionCount { get; set; }
}

public sealed class CreateAssessmentRequest
{
    public required string Title { get; set; }
    public string? Subject { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public decimal TotalMarks { get; set; }
    public List<string>? QuestionIds { get; set; }
    public List<string>? Tags { get; set; }
}

public sealed class AttendanceDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Section { get; set; }
    public List<string> AbsenteeStudentIds { get; set; } = [];
    public int AbsenteeCount { get; set; }
}

public sealed class PostAttendanceRequest
{
    public DateTime Date { get; set; }
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Section { get; set; }
    public List<string> AbsenteeStudentIds { get; set; } = [];
}

public sealed class GradingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public List<GradingRuleDto> Rules { get; set; } = [];
}

public sealed class GradingRuleDto
{
    public string Grade { get; set; } = "";
    public decimal MinPercent { get; set; }
    public decimal MaxPercent { get; set; }
}

public sealed class IdRequest { public required Guid Id { get; set; } }
public sealed class StringIdRequest { public required string Id { get; set; } }
public sealed class NameRequest { public required Guid Id { get; set; } public required string Name { get; set; } }
public sealed class TagRequest { public required string Name { get; set; } }
