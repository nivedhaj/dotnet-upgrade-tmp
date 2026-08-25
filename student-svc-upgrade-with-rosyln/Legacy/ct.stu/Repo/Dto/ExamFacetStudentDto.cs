using System.Collections.Generic;

namespace Repo.Dto
{
    internal class ExamFacetStudentDto
    {
        public string InstId { get; set; }
        public string CourseId { get; set; }
        public List<EF_SectionDetails> Sections { get; set; }

        public ExamFacetStudentDto()
        {
            Sections = new List<EF_SectionDetails>();
        }
    }

    internal class EF_SectionDetails
    {
        public string SectionName { get; set; }
        public List<EF_StudentDetail> StudentList { get; set; }
    }

    internal class EF_StudentDetail
    {
        public string StudentId { get; set; }
        public string Name { get; set; }
    }
}