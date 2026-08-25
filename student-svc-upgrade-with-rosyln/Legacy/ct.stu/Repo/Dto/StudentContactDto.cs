using System.Collections.Generic;
using Repo.Resource;

namespace Repo.Dto
{
    /// <summary>
    /// Student Contact
    /// </summary>
    public class StudentContactDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// Student Id
        /// </summary>
        /// <value></value>
        public string StudentId { get; set; }
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public List<string> Emails { get; set; }
        public List<string> Mobiles { get; set; }
        public EnrollmentStatusEnum Status { get; internal set; }
    }
}