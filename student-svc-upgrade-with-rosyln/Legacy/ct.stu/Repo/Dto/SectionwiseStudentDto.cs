using System.Collections.Generic;

namespace Repo.Dto
{
    internal class SectionwiseStudentDto
    {
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string SectionName { get; set; }
        public List<SectionwiseStudentDetailsDto> StudentList { get; set; }

        public SectionwiseStudentDto()
        {
            StudentList = new List<SectionwiseStudentDetailsDto>();
        }
    }

    internal class SectionwiseStudentDetailsDto
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentGender { get; set; }
        public string EnrollmentStatus { get; set; }
        public int RollNo { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }
}