using System;
using System.Collections.Generic;
using ct._Core.Repo.Resource;
using Repo.Resource;

namespace Repo.Dto
{
    internal class StudentParentDetailsRptDto
    {
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }

        public string SectionName { get; set; }
        public List<StudentParentDetailsDto> StudentList { get; set; }

        public StudentParentDetailsRptDto()
        {
            StudentList = new List<StudentParentDetailsDto>();
        }
    }

    internal class StudentParentDetailsDto
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Mobiles { get; set; }
        public string Emails { get; set; }
    }

    public class SPD_CSV
    {
        public string Columns { get; set; }
        public List<SPD_Dto> Students { get; set; }
        public SPD_CSV()
        {
            Students = new List<SPD_Dto>();
        }
    }

    public class SPD_Dto
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Mobiles { get; set; }
        public string Emails { get; set; }
    }
}