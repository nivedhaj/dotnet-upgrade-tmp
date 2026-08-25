using System;
using ct._Core.Repo.Resource;
using Repo.Resource;

namespace Repo.Dto
{

    /// <summary>
    /// Student flat dto
    /// </summary>
    public class StudentFlatDto
    {
        public string Id { get; set; }
        public string InstId { get; set; }
        public string Name { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string CourseId { get; set; }
        public string Course { get; set; }
        public string Term { get; set; }
        public string Section { get; set; }
        public string Branch { get; set;}
        public string StudentRefId { get; set; }
        public string StudentMobile { get; set; }
        public string FatherMobile { get; set; }
        public string MotherMobile { get; set; }
        public string GuardianMobile { get; set; }
        public string StudentEmail { get; set; }
        public string FatherEmail { get; set; }
        public string MotherEmail { get; set; }
        public string GuardianEmail { get; set; }
        public string VirtualId { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
    }
}