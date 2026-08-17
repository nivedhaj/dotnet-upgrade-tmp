using Repo.Resource;
using ct._Core.Repo.Resource;

namespace Repo.Db.Index;

/// <summary>PostgreSQL projection marker replacing the former document index.</summary>
internal sealed class Student_Inst_Index
{
    public class Result : Student
    {
        public string GenderAsString => Gender?.ToString() ?? string.Empty;
        public string StatusAsString => Status.ToString();
        public string InstId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string CourseNameAndBranch => $"{CourseName} {Branch}".Trim();
        public int CourseSortIndex { get; set; }
        public string TermName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public EnrollmentStatusEnum EnrollmentStatus { get; set; }
        public string EnrollmentStatusAsString => EnrollmentStatus.ToString();
        public int RollNo { get; set; }
        public DateTime DOA { get; set; }
        public string TagsCsv => string.Join(",", Tags ?? new List<string>());
        public string Emails { get; set; } = string.Empty;
        public List<string> EmailList { get; set; } = new();
        public string Mobiles { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string FatherPAN { get; set; } = string.Empty;
        public string MotherPAN { get; set; } = string.Empty;
        public string OrgId { get; set; } = string.Empty;
        public string AdmissionNo { get; set; } = string.Empty;
    }
}

internal sealed class Institute
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
}

internal sealed class Student_Search
{
    public class ReduceResult
    {
        public string Query { get; set; } = string.Empty;
        public StudentStatusEnum Status { get; set; }
    }
}

internal sealed class Student_Coursewise_Count
{
    public class Result
    {
        public string InstId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public int Boys { get; set; }
        public int Girls { get; set; }
        public int Total => Boys + Girls;
    }
}

internal sealed class Inst_Student_Count
{
    public class Result
    {
        public string InstId { get; set; } = string.Empty;
        public int StudentCountMale { get; set; }
        public int StudentCountFemale { get; set; }
        public int TotalStudentCount => StudentCountMale + StudentCountFemale;
    }
}
