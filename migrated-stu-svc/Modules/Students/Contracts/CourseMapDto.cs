using ct._Core.Repo.Resource;
using Repo.Resource;

namespace Repo.Dto
{
    /// <summary>
    /// Course Map
    /// </summary>
    internal class CourseMap
    {
        public string BaseCourseId { get; set; }
        public string TargetCourseId { get; set; }
        public string BaseInstId { get; set; }
        public string TargetInstId { get; set; }
    }
}