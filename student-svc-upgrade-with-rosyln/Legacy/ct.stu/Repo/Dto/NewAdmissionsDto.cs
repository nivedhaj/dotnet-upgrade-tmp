using System;
using System.Collections.Generic;

namespace Repo.Dto
{
    internal class NewAdmissionsDto
    {
        public List<NewAdmission_StudentDetailsDto> StudentList { get; set; }
        public NewAdmissionsDto()
        {
            StudentList = new List<NewAdmission_StudentDetailsDto>();
        }
    }

    internal class NewAdmission_StudentDetailsDto
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string EnrollmentStatus { get; set; }
        public string StudentGender { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string SectionName { get; set; }
        public string InstId { get; set; }
        public DateTime DOA { get; set; }
public List<string> Tags { get; set; }

public string GenderAsString { get; set; }
    }
}