using System.Collections.Generic;
using System.Linq;

internal class CoursewiseStudentDto
{
    public string InstId { get; set; }
    public string CourseId { get; set; }
    public string CourseName { get; set; }
    public List<SectionDetail> Sections { get; set; }
    public string Status { get; set; }
    public int TotalBoys { get { return this.Sections.Sum(s => s.NumberOfBoys); } }
    public int TotalGirls { get { return this.Sections.Sum(s => s.NumberOfGirls); } }
    public int TotalStudents { get { return TotalBoys + TotalGirls; } }
    public int TotalCourseStudents { get { return TotalBoys + TotalGirls; } }

    public CoursewiseStudentDto()
    {
        Sections = new List<SectionDetail>();
    }
}

internal class SectionDetail
{
    public string SectionName { get; set; }
    public int NumberOfBoys { get; set; }
    public int NumberOfGirls { get; set; }
    public int TotalSectionStudents { get { return NumberOfBoys + NumberOfGirls; } }
}