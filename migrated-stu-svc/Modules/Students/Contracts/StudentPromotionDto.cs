namespace Repo.Dto
{

    /// <summary>
    /// Student Promotion dto
    /// </summary>
    public class StuPromotionDto
    {
        public string StudentId { get; set; }
        public string BaseInstId { get; set; }
        public string BaseCourseId { get; set; }
        public string BaseCourseName { get; set; }
        public string TargetCourseName { get; set; }
        public string BaseTerm { get; set; }
        public string BaseSection { get; set; }
        public string TargetInstId { get; set; }
        public string TargetCourseId { get; set; }
        public string TargetTerm { get; set; }
        public string TargetSection { get; set; }
    }
}