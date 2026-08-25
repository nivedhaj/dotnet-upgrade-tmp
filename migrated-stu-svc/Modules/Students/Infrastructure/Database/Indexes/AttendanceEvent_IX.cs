using Repo.Resource;
namespace Repo.Db.Index;
public sealed class AttendanceEvent_IX
{
    public sealed class Result
    {
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public string StudentId { get; set; }
        public DateTime Date { get; set; }
    }
}
