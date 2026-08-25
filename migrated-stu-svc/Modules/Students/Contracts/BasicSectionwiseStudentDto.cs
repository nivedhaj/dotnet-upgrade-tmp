using System.Collections.Generic;

namespace Repo.Dto
{
    internal class BS_StudentDto
    {
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string SectionName { get; set; }
        public List<BS_StudentDetailsDto> StudentList { get; set; }

        public BS_StudentDto()
        {
            StudentList = new List<BS_StudentDetailsDto>();
        }
    }

    internal class BS_StudentDetailsDto
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string EnrollmentStatus { get; set; }
        public string StudentGender { get; set; }
    }
}