using System;
using System.Collections.Generic;

namespace Repo.Dto
{
    /// <summary>
    /// Absentees report dto
    /// </summary>
    public class AbsenteesReportDto
    {
        /// <summary>
        /// Institute id
        /// </summary>
        /// <returns></returns>
        public string InstId { get; set; }
        /// <summary>
        /// Attendance date
        /// </summary>
        /// <returns></returns>
        public DateTime AttendanceDate { get; set; }
        /// <summary>
        /// Attendance datet as a string
        /// </summary>
        /// <returns></returns>
        public string AttendanceTakenOn { get { return string.Format("{0:dd MMM yyyy}", AttendanceDate); } }
        /// <summary>
        /// Student list
        /// </summary>
        /// <returns></returns>
        public List<AbsentStudentDto> StudentList { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        public AbsenteesReportDto()
        {
            StudentList = new List<AbsentStudentDto>();
        }
    }

    /// <summary>
    /// Absent student dto
    /// </summary>
    public class AbsentStudentDto
    {
        /// <summary>
        /// Course id
        /// </summary>
        /// <returns></returns>
        public string CourseId { get; set; }
        /// <summary>
        /// Course
        /// </summary>
        /// <returns></returns>
        public string Course { get; set; }
        /// <summary>
        /// Section
        /// </summary>
        /// <returns></returns>
        public string Section { get; set; }
        /// <summary>
        /// Student id
        /// </summary>
        /// <returns></returns>
        public string StudentId { get; set; }
        /// <summary>
        /// Student name
        /// </summary>
        /// <returns></returns>
        public string StudentName { get; set; }
    }
}