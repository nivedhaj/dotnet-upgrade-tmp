using Repo.Resource;
namespace Repo.Db.Index;
public sealed class Coursewise_Sectionwise_Student_Index { }
public sealed class Course_Structure_Index
{
    public sealed class Result
    {
        public string Name { get; set; }
        public string Branch { get; set; }
        public string Id { get; set; }
        public string InstId { get; set; }
        public CourseStatusEnum Status { get; set; }
    }
}
