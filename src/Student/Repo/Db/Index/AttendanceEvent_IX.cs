// Migrated from RavenDB index → Postgres/Marten.
// The nested Result types are kept for query projections.
// Raven Map/Reduce definitions are no longer executed; use Marten LINQ
// against documents or configure property indexes in StudentModule CreateStore.
using System;
using System.Linq;
using Marten.Schema;
using Repo.Resource;

namespace Repo.Db.Index
{
    public class AttendanceEvent_IX 
    {
        public class Result
        {
            public string InstId { get; set; }
            public string CourseId { get; set; }
            public string StudentId { get; set; }
            public DateTime Date { get; set; }

        }
        public AttendanceEvent_IX()
        {
            Map =
                attendance_events =>
                from att in attendance_events
                select new Result
                {
                    InstId = att.InstId,
                    CourseId = att.CourseId,
                    StudentId = att.StudentId,
                    Date = att.Date
                };
        }
    }
}