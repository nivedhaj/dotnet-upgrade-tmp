using ct._Core.Repo.Resource;
using Repo.Resource;

namespace Repo.Db.Index;

internal sealed class Student_Inst_Index
{
    public sealed class Result
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public GenderEnum? Gender { get; set; }
        public string GenderAsString => Gender?.ToString() ?? string.Empty;
        public StudentStatusEnum Status { get; set; }
        public string StatusAsString => Status.ToString();
        public string InstId { get; set; }
        public string StudentId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string Branch { get; set; }
        public string CourseNameAndBranch => $"{CourseName} {Branch ?? string.Empty}".Trim();
        public int CourseSortIndex { get; set; }
        public string TermName { get; set; }
        public string SectionName { get; set; }
        public EnrollmentStatusEnum EnrollmentStatus { get; set; }
        public string EnrollmentStatusAsString => EnrollmentStatus.ToString();
        public int RollNo { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime DOA { get; set; }
        public List<string> Tags { get; set; } = new();
        public string Emails { get; set; }
        public List<string> EmailList { get; set; }
        public string Mobiles { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string FatherPAN { get; set; }
        public string MotherPAN { get; set; }
        public string VirtualId { get; set; }
        public string OrgId { get; set; }
        public string UserId { get; set; }
        public string AdmissionNo { get; set; }
        public List<FeesReceivable> FeesReceivable { get; set; } = new();
    }
}

internal sealed class Student_Search
{
    public sealed class ReduceResult { public string Query { get; set; } public StudentStatusEnum Status { get; set; } }
}

internal sealed class Student_Coursewise_Count
{
    public sealed class Result
    {
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string TermName { get; set; }
        public string SectionName { get; set; }
        public int Boys { get; set; }
        public int Girls { get; set; }
        public int Total => Boys + Girls;
    }
}

internal sealed class Inst_Student_Count
{
    public sealed class Result
    {
        public string InstId { get; set; }
        public int StudentCountMale { get; set; }
        public int StudentCountFemale { get; set; }
        public int TotalStudentCount => StudentCountMale + StudentCountFemale;
    }
}
