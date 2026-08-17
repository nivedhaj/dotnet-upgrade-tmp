using Repo.Resource;

namespace Repo.Db.Index;

public sealed class Coursewise_Sectionwise_Student_Index
{
    public class Result
    {
        public string InstId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public CourseStatusEnum Status { get; set; }
        public string SectionName { get; set; } = string.Empty;
    }
}

public sealed class Course_Structure_Index
{
    public class Result
    {
        public string Name { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string InstId { get; set; } = string.Empty;
        public CourseStatusEnum Status { get; set; }
    }
}
