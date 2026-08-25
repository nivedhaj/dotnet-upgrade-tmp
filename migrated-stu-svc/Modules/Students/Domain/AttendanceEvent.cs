using System;
using ct._Core.Repo.Resource;

namespace Repo.Resource
{
    public class AttendanceEvent : Entity
    {
        public string Id { get; set; }
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public string TermName { get; set; }
        public string SectionName { get; set; }
        public DateTime Date { get; set; }
        public int PeriodNo { get; set; }
        public string SubjectName { get; set; }
        public bool IsOptionalSubject { get; set; }
        public string StudentId { get; set; }
        public string StaffId { get; set; }
        public string Attendance { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }

        public AttendanceEvent()
        {
            CreatedOn = DateTime.Now;
        }
    }
}
