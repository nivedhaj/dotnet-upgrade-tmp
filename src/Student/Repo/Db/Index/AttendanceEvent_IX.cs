using Repo.Resource;

namespace Repo.Db.Index;

public sealed class AttendanceEvent_IX
{
    public class Result : AttendanceEvent
    {
        public string InstIdValue { get; set; } = string.Empty;
        // The source aggregate already contains InstId/CourseId/StudentId/Date.
    }
}
