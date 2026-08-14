using System;
using System.Collections.Generic;
using ct._Core.Repo.Resource;
using Repo.Resource;

namespace Repo.Dto
{
    internal class StudentDetailsRptDto
    {
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }

        public string SectionName { get; set; }
        public List<StudentDetailsDto> StudentList { get; set; }

        public StudentDetailsRptDto()
        {
            StudentList = new List<StudentDetailsDto>();
        }
    }

    internal class StudentDetailsDto
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StudentGender { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public string DOB { get; set; }
        public int SENDCategory { get; set; }
        public string Nationality { get; set; }
        public int CountryCode { get; set; }
        public string RollNo { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }

    internal class Rpt_StudentDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public Person Father { get; set; }
        public Person Mother { get; set; }
        public string InstId { get; set; }
        public string StudentId { get; set; }
        public string Emails { get; set; }
        public string Mobiles { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string Branch { get; set; }
        public string SectionName { get; set; }
        public string RollNo { get; set; }
    }
}