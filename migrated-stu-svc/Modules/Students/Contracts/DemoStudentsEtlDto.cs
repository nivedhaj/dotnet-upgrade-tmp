using System.Collections.Generic;

namespace Repo.Dto
{
    /// <summary>
    /// Data dto
    /// </summary>
    public class FirstName
        {
            public string first_name { get; set; }
            public string gender { get; set; }
        }

        public class CourseAge
        {
            public string Id { get; set; }
            public string CourseName { get; set; }
            public int Age { get; set; }
            public string Branch { get; set; }
            public string Term { get; set; }
            public List<EtlSection> Sections { get; set; }
            public CourseAge()
            {
                Sections = new List<EtlSection>();
            }
        }

        public class EtlSection
        {
            public string Name { get; set; }
        }
}