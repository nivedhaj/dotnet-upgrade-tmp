using System.Collections.Generic;
using ct._Core.Security;
using Microsoft.Extensions.Configuration;
using Repo.Db.Query;

namespace ct.stu.Service
{
    /// <summary>
    /// Student Service
    /// </summary>
    public class StudentService
    {
        IUserContext _uc;
        IConfiguration _configuration { get; }
        private readonly StudentQry _studentQry;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="uc"></param>
        /// <param name="studentQry"></param>
        public StudentService(IConfiguration configuration, IUserContext uc, StudentQry studentQry)
        {
            _configuration = configuration;
            _uc = uc.Current();
            _studentQry = studentQry;
        }

        /// <summary>
        /// Get Student Dto
        /// </summary>
        /// <returns></returns>
        public StudentDto GetDto()
        {
            return new StudentDto();
        }

        /// <summary>
        /// Get Student Dto
        /// </summary>
        /// <returns></returns>
        public List<StudentDto> GetDtoWithData()
        {
            var students = _studentQry.GetDtoWithData();
            var stu_list = new List<StudentDto>();
            foreach (var stu in students)
            {
                var dto = new StudentDto();
                dto.id = stu.Id;
                dto.name = stu.Name;
                dto.gender = stu.GenderAsString;
                dto.course_name = stu.CourseNameAndBranch;
                dto.student_id = stu.StudentId;
                dto.email = !string.IsNullOrEmpty(stu.Emails) ? stu.Emails.Split(",")[0] : string.Empty;
                dto.mobile = !string.IsNullOrEmpty(stu.Mobiles) ? stu.Mobiles.Split(",")[0] : string.Empty;
                dto.roll_no = stu.RollNo;
                dto.tags = stu.Tags != null && stu.Tags.Count != 0 ? string.Format("\"{0}\"", string.Join(",", stu.Tags.ToArray())) : string.Empty;
                stu_list.Add(dto);
            }
            return stu_list;
        }
    }

    public class StudentDto
    {
        public string name { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string course_name { get; set; }
        public string section_name { get; set; }
        public string student_id { get; set; }
        public string doa { get; set; }

        public string aadhar_number { get; set; }

        // public string course_id { get; set; }
        public string email { get; set; }
        public string mobile;

        // public string father_name { get; set; }
        // public string mother_name { get; set; }
        // public string guardian_name { get; set; }
        public int roll_no { get; set; }
        public string tags { get; set; }
        public string id { get; set; }

        public string inst_id { get; set; }
    }
}