using System;
using System.Linq;
using Raven.Client.Documents.Indexes;
using Repo.Resource;

namespace Repo.Db.Index
{
    public class AttendanceEvent_IX : AbstractIndexCreationTask<AttendanceEvent>
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