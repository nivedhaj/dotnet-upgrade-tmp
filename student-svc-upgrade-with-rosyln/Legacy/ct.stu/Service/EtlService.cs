using ct._Core.Repo.Resource;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Newtonsoft.Json;
using Repo.Db.Command;
using Repo.Db.Query;
using Repo.Dto;
using Repo.Resource;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ct.stu.Service
{
    /// <summary>
    /// Extract Transform Load (ETL) Service Utility functions.
    /// </summary>
    public class EtlService
    {

        private readonly IUserContext _uc;
        private readonly AWSService _awsService;
        private readonly StudentQry _studentQry;
        private readonly StudentCmd _studentCmd;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="awsService"></param>
        /// <param name="studentQry"></param>
        /// <param name="studentCmd"></param>
        public EtlService(IUserContext uc = null, AWSService awsService = null, StudentQry studentQry = null, StudentCmd studentCmd = null)
        {
            _uc = uc.Current();
            _awsService = awsService;
            _studentQry = studentQry;
            _studentCmd = studentCmd;
        }

        /// <summary>
        /// Get Api Script that can be sent to etl worker to execute
        /// </summary>
        /// <param name="verb">Defaults to post</param>
        /// <param name="endPoint">Provide path relative to api. Ex /api/auth/org</param>
        /// <param name="payload"></param>
        /// <param name="separator">Defaults to tab \u0009</param>
        /// <returns></returns>
        public string GetApiScript(string endPoint, object payload, string separator = "\u0009", string verb = "post")
        {
            return $"{verb}{separator}{endPoint}{separator}{JsonConvert.SerializeObject(payload)}";
        }

        public string GetApiScriptJsonl(string endPoint, object payload, string separator = "\u0009", string verb = "post")
        {
            return $"{{\"http_method\":\"{verb}\",\"endpoint\":\"{endPoint}\",\"payload\":{JsonConvert.SerializeObject(payload)}}}";
        }

        /// <summary>
        /// Generate Target Id for a base_id (guid as string)
        /// </summary>
        /// <param name="base_id"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string GetTargetId(string base_id, string year)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(base_id, out guid);
            if (guid == Guid.Empty) return "";

            var arr = base_id.Split("-");
            var target_id = $"{arr[0]}-{year}-{arr[2]}-{arr[3]}-{arr[4]}";
            return target_id;
        }
        /// <summary>
        /// S3 Path for Org/Inst/Personas/Users (OIPU)
        /// </summary>
        /// <param name="shortName">Short Name of the Inst</param>
        /// <param name="targetYear">Year range. Ex 1920, 2021 etc</param>
        /// <param name="env">prod / test. Defaults to test</param>
        /// <returns></returns>
        public string GetS3PathStudents(string shortName, string targetYear, string env = "test")
        {
            string file_name = GetEtlFileName(shortName, targetYear, env);
            string script_file_path = $"{shortName.ToLower()}/{file_name}";
            return script_file_path.ToLower();
        }

        /// <summary>
        /// Etl file name to be stored in S3
        /// </summary>
        /// <param name="shortName">Short Name of the Inst</param>
        /// <param name="targetYear">Year range. Ex 1920, 2021 etc</param>
        /// <param name="env">prod / test. Defaults to test</param>
        /// <returns></returns>
        public string GetEtlFileName(string shortName, string targetYear, string env = "test")
        {
            string prefix = $"{env}_05";
            string file_name = $"{prefix}_{shortName}_{targetYear}_students_promotion.etl";
            return file_name.ToLower();
        }

        /// <summary>
        /// Etl file name to be stored in S3
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="targetYear"></param>
        /// <param name="courseName"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public string GetEtlFileName_NewStudents(string shortName, string targetYear, string courseName, string env = "test")
        {
            string prefix = $"{env}_09";
            string file_name = $"{prefix}_{shortName}_{targetYear}_{courseName}_students.etl";
            return file_name.ToLower();
        }

        public string GetEtlFileName_DemoStudents(string short_name, string acad_start_year)
        {
            string file_name = $"{short_name}_{acad_start_year}_demo_students.etl";
            return file_name.ToLower();
        }

        public string GetEtlFileName_Opening_balance(string shortName, string targetYear, string env = "test")
        {
            string prefix = $"{env}_11";
            string file_name = $"{prefix}_{shortName}_{targetYear}_opening_balance.etl";
            return file_name.ToLower();
        }

        internal void WriteScriptToS3(string fileName, string content)
        {
            // Upload to s3
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            _awsService.Upload(fileName, stream);
        }

        internal List<string> GetApiScript_PromoteStudents(string env, string shortName, string targetYear, string baseInstId = "")
        {
            var result = new List<string>();
            string prefix = $"{env}_04";
            string file_name = $"{prefix}_{shortName}_{targetYear}_coursemaps.json";
            var course_maps_file = _awsService.GetFileContent(file_name, targetYear, ignoreSuffix: true);
            List<CourseMap> course_maps = JsonConvert.DeserializeObject<List<CourseMap>>(course_maps_file);
            var students = _studentQry.GetAll("1a6a3029-2324-45ec-9500-f37f54f82d5f");
            var students_to_promote = new List<StuPromotionDto>();
            foreach (var stu in students)
            {
                foreach (var cm in course_maps)
                {
                    if (cm.BaseCourseId == stu.CourseId)
                    {
                        var stu_pro_dto = new StuPromotionDto();
                        stu_pro_dto.StudentId = stu.Id;
                        stu_pro_dto.BaseCourseId = cm.BaseCourseId;
                        stu_pro_dto.TargetCourseId = cm.TargetCourseId;
                        stu_pro_dto.BaseInstId = cm.BaseInstId;
                        stu_pro_dto.TargetInstId = cm.TargetInstId;
                        stu_pro_dto.BaseTerm = stu.TermName;
                        stu_pro_dto.TargetTerm = stu.TermName;
                        stu_pro_dto.BaseSection = stu.SectionName;
                        stu_pro_dto.TargetSection = stu.SectionName == "Yellow" ? "A" : stu.SectionName == "Blue" ? "B" : stu.SectionName == "Red" ? "C" : stu.SectionName == "Green" ? "D" : stu.SectionName;
                       stu_pro_dto.BaseCourseName = stu.CourseName;
                       stu_pro_dto.TargetCourseName = stu.CourseName == "Playhome" ? "KG I" : stu.CourseName == "KG I" ? "KG II" : stu.CourseName == "KG II" ? "I" : stu.CourseName == "I" ? "II" : stu.CourseName == "II" ? "III" : stu.CourseName == "III" ? "IV" : stu.CourseName == "IV" ? "V" : stu.CourseName == "V" ? "VI" : stu.CourseName == "VI" ? "VII" : string.Empty;
                        students_to_promote.Add(stu_pro_dto);
                    }
                }
            }
            foreach (var stu in students_to_promote)
            {
                if (!string.IsNullOrEmpty(stu.TargetCourseId) || !string.IsNullOrEmpty(stu.TargetInstId))
                {
                    var etl_line = get_etl_student_promotion(stu);
                    result.Add(etl_line);
                }
            }
            return result;
        }
        private string get_etl_student_promotion(StuPromotionDto stuDto)
        {
            var end_point = @"/api/stu/student/promote";
            var payload = new StuPromotionDto();
            payload.StudentId = stuDto.StudentId;
            payload.BaseCourseId = stuDto.BaseCourseId;
            payload.TargetCourseId = stuDto.TargetCourseId;
            payload.BaseInstId = stuDto.BaseInstId;
            payload.TargetInstId = stuDto.TargetInstId;
            payload.BaseTerm = stuDto.BaseTerm;
            payload.TargetTerm = stuDto.TargetTerm;
            payload.BaseSection = stuDto.BaseSection;
            payload.TargetSection = stuDto.TargetSection;
            payload.BaseCourseName = stuDto.BaseCourseName;
            payload.TargetCourseName = stuDto.TargetCourseName;
            var api_post = GetApiScript(end_point, payload);
            return api_post;
        }

        internal List<string> GetApiScript_NewStudentsImport(string filename, string course_id, string term, string inst_id)
        {
            var result = new List<string>();
            string file = Path.Combine(Path.GetTempPath(), filename);
            var csv = File.ReadAllLines(file);
            var student_list = Helper.CsvToList<StudentData>(csv);
            var students = new List<CreateStudentDto>();
            var students_father_details = new List<UpdateFatherDetailsStudentDto>();
            var students_mother_details = new List<UpdateMotherDetailsStudentDto>();
            // var students_guardian_details = new List<UpdateGuardianDetailsStudentDto>();
            //var students_primary_mobile = new List<SetPrimaryMobileDto>();
            foreach (var student in student_list)
            {
                DateTime dob;
                var new_guid = Guid.NewGuid();
                var stu_id = new_guid.ToString();
                var stu = new CreateStudentDto();
                stu.Id = stu_id;
                stu.Name = string.IsNullOrWhiteSpace(student.name)
    ? string.Empty
    : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
        student.name
            .Replace(".", " ")
            .Trim()
            .ToLower()
      ).Trim();
                stu.CourseName = student.course.Trim();
                stu.CourseId = course_id;
                stu.DOB = string.IsNullOrWhiteSpace(student.dob)
    ? DateTime.MinValue
    : DateTime.ParseExact(
        student.dob,
        "d/M/yyyy",
        CultureInfo.InvariantCulture);
                stu.Gender = student.gender.ToLower().Trim() == "m" ? GenderEnum.Male : GenderEnum.Female;
                stu.SecName = student.section.Trim();
                stu.Branch = student.branch;
                stu.StudentId = student.student_id;
                stu.InstId = inst_id;
                stu.TermName = term;
                students.Add(stu);

                var stu_father_details = new UpdateFatherDetailsStudentDto();
                stu_father_details.Id = stu_id;
                stu_father_details.Name = string.IsNullOrWhiteSpace(student.father_name)
    ? string.Empty
    : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
        student.father_name
            .Replace(".", " ")
            .Trim()
            .ToLower()
      ).Trim();
                stu_father_details.Email = student.father_email;
                stu_father_details.Mobile = student.father_mobile;
                students_father_details.Add(stu_father_details);

                var stu_mother_details = new UpdateMotherDetailsStudentDto();
                stu_mother_details.Id = stu_id;
                stu_mother_details.Name = string.IsNullOrWhiteSpace(student.mother_name)
    ? string.Empty
    : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
        student.mother_name
            .Replace(".", " ")
            .Trim()
            .ToLower()
      ).Trim();
                stu_mother_details.Email = student.mother_email;
                stu_mother_details.Mobile = student.mother_mobile;
                students_mother_details.Add(stu_mother_details);

                // var stu_guardian_details = new UpdateGuardianDetailsStudentDto();
                // stu_guardian_details.Id = stu_id;
                // stu_guardian_details.Name = student.guardian_name;
                // stu_guardian_details.Email = student.guardian_email;
                // stu_guardian_details.Mobile = student.guardian_mobile;
                // students_guardian_details.Add(stu_guardian_details);

                // var stu_primary_mobile = new SetPrimaryMobileDto();
                // stu_primary_mobile.Id = stu_id;
                // stu_primary_mobile.Mobile = student.mobile;
                // students_primary_mobile.Add(stu_primary_mobile);
            }

            foreach (var s in students)
            {
                var etl_line = get_etl_new_student(s);
                result.Add(etl_line);
            }

            foreach (var stu_father_detail in students_father_details)
            {
                var etl_line_father_details = get_etl_new_student_father_details(stu_father_detail);
                result.Add(etl_line_father_details);
            }

            foreach (var stu_mother_detail in students_mother_details)
            {
                var etl_line_mother_details = get_etl_new_student_mother_details(stu_mother_detail);
                result.Add(etl_line_mother_details);
            }

            // foreach (var stu_guardian_detail in students_guardian_details)
            // {
            //     if (!string.IsNullOrEmpty(stu_guardian_detail.Name))
            //     {
            //         var etl_line_guardian_details = get_etl_new_student_guardian_details(stu_guardian_detail);
            //         result.Add(etl_line_guardian_details);
            //     }
            // }

            // foreach (var stu_primary_mobile in students_primary_mobile)
            // {
            //     if (!string.IsNullOrEmpty(stu_primary_mobile.Mobile))
            //     {
            //         var etl_line_mobile_details = get_etl_to_set_primary_mobile(stu_primary_mobile);
            //         result.Add(etl_line_mobile_details);
            //     }
            // }

            return result; ;

        }

        internal List<string> GetApiScript_StudentsEtl(List<CreateStudentDto> students)
        {
            var result = new List<string>();
            foreach (var s in students)
            {
                var etl_line = get_etl_new_student(s);
                result.Add(etl_line);
            }
            return result;
        }

         internal List<string> GetApiScript_StudentsEtlJsonl(List<CreateStudentDto> students)
        {
            var result = new List<string>();
            foreach (var s in students)
            {
                var etl_line = get_etl_new_student_jsonl(s);
                result.Add(etl_line);
            }
            return result;
        }

        internal List<string> GetApiScript_UpdateFatherDetails(List<UpdateFatherDetailsStudentDto> students)
        {
            var result = new List<string>();
            foreach (var s in students)
            {
                var etl_line = get_etl_new_student_father_details(s);
                result.Add(etl_line);
            }
            return result;
        }

        internal List<string> GetApiScript_UpdateStudentMobile(List<SetPrimaryMobileDto> students)
        {
            var result = new List<string>();
            foreach (var s in students)
            {
                var etl_line = get_etl_to_set_primary_mobile(s);
                result.Add(etl_line);
            }
            return result;
        }

        internal List<string> GetApiScript_OpeningBalance(List<FeesReceivableDto> students)
        {
            var result = new List<string>();
            foreach (var s in students)
            {
                var etl_line = get_etl_line_opening_balance(s);
                result.Add(etl_line);
            }
            return result;
        }

        private string get_etl_line_opening_balance(FeesReceivableDto student)
        {
            var end_point = @"/api/stu/student/feesReceivable";
            var payload = new FeesReceivableDto();
            payload.Id = student.Id;
            payload.InstId = student.InstId;
            payload.Amount = student.Amount;
            var api_post = GetApiScript(end_point, payload);
            return api_post;
        }
        private string get_etl_new_student(CreateStudentDto studentDto)
        {
            var end_point = @"/api/stu/student";
            var payload = new CreateStudentDto();
            payload.Id = studentDto.Id;
            payload.InstId = studentDto.InstId;
            payload.Name = studentDto.Name;
            payload.CourseId = studentDto.CourseId;
            payload.CourseName = studentDto.CourseName;
            payload.SecName = studentDto.SecName;
            payload.TermName = studentDto.TermName;
            payload.Branch = studentDto.Branch;
            payload.DOB = studentDto.DOB;
            payload.Gender = studentDto.Gender;
            payload.StudentId = studentDto.StudentId;
            var api_post = GetApiScript(end_point, payload);
            return api_post;
        }

         private string get_etl_new_student_jsonl(CreateStudentDto studentDto)
        {
            var end_point = @"/api/stu/student";
            var payload = new CreateStudentDto();
            payload.Id = studentDto.Id;
            payload.InstId = studentDto.InstId;
            payload.Name = studentDto.Name;
            payload.CourseId = studentDto.CourseId;
            payload.CourseName = studentDto.CourseName;
            payload.SecName = studentDto.SecName;
            payload.TermName = studentDto.TermName;
            payload.Branch = studentDto.Branch;
            payload.DOB = studentDto.DOB;
            payload.Gender = studentDto.Gender;
            payload.StudentId = studentDto.StudentId;
            var api_post = GetApiScriptJsonl(end_point, payload);
            return api_post;
        }

        private string get_etl_new_student_father_details(UpdateFatherDetailsStudentDto fatherDetailsDto)
        {
            var end_point = @"/api/stu/student/updateFatherDetails";
            var payload = new UpdateFatherDetailsStudentDto();
            payload.Id = fatherDetailsDto.Id;
            payload.Name = fatherDetailsDto.Name;
            payload.Email = fatherDetailsDto.Email;
            payload.Mobile = fatherDetailsDto.Mobile;
            var api_post = GetApiScript(end_point, payload);
            return api_post;
        }

        private string get_etl_to_set_primary_mobile(SetPrimaryMobileDto student)
        {
            var end_point = @"/api/stu/student/setPrimaryMobile";
            var payload = new SetPrimaryMobileDto();
            payload.Id = student.Id;
            payload.Mobile = student.Mobile;
            var api_post = GetApiScript(end_point, payload);
            return api_post;
        }

        private string get_etl_new_student_mother_details(UpdateMotherDetailsStudentDto motherDetailsDto)
        {
            var end_point = @"/api/stu/student/updateMotherDetails";
            var payload = new UpdateMotherDetailsStudentDto();
            payload.Id = motherDetailsDto.Id;
            payload.Name = motherDetailsDto.Name;
            payload.Email = motherDetailsDto.Email;
            payload.Mobile = motherDetailsDto.Mobile;
            var api_post = GetApiScript(end_point, payload);
            return api_post;
        }

        private string get_etl_new_student_guardian_details(UpdateGuardianDetailsStudentDto guardianDetailsDto)
        {
            var end_point = @"/api/stu/student/updateGuardianDetails";
            var payload = new UpdateFatherDetailsStudentDto();
            payload.Id = guardianDetailsDto.Id;
            payload.Name = guardianDetailsDto.Name;
            payload.Email = guardianDetailsDto.Email;
            payload.Mobile = guardianDetailsDto.Mobile;
            var api_post = GetApiScript(end_point, payload);
            return api_post;
        }
        public int[] Shuffle(int n)
        {
            var randomGenerator = new Random();
            return Enumerable.Range(0, n).OrderBy(x => randomGenerator.Next()).ToArray();
        }

        internal List<string> get_etl_students(List<Course> courses, Guid instIdGuid)
        {
            var result = new List<string>();

            // assign students
            var first_names = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "firstname.csv"));
            var last_names = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "lastname.csv"));
            var f_list = Helper.CsvToList<FirstName>(first_names);
            var f_list_string = JsonConvert.SerializeObject(f_list);
            var f_json = JsonConvert.DeserializeObject<List<FirstName>>(f_list_string);
            var boys = f_json.Where(o => o.gender == "Male");
            var girls = f_json.Where(o => o.gender == "Female");

            var boys_index = Shuffle(boys.Count());
            var girls_index = Shuffle(girls.Count());
            var ln_index = 1;
            var boy_names = new List<string>();
            var girl_names = new List<string>();

            foreach (var i in boys_index)
            {
                var b_list = boys.ToList();
                var item = b_list[i].first_name;
                if (item != last_names[ln_index])
                {
                    string name = item + ' ' + last_names[ln_index];
                    if ((last_names.Length - 1) != ln_index) ln_index++;
                    else ln_index = 1;
                    boy_names.Add(name);
                }
            }
            foreach (var i in girls_index)
            {
                var b_list = girls.ToList();
                var item = b_list[i].first_name;
                if (item != last_names[ln_index])
                {
                    string name = item + ' ' + last_names[ln_index];
                    if ((last_names.Length - 1) != ln_index) ln_index++;
                    else ln_index = 1;
                    girl_names.Add(name);
                }
            }
            var studs = new List<CreateStudentDto>();

            var course_iteration = 1;
            var section_iteration = 0;
            var stu_iteration = 0;
            var dob_year = 13;

            foreach (var course in courses)
            {
                var course_id = GuidHelper.GetCourseId(instIdGuid, course.Name);
                foreach (var term in course.Terms)
                {
                    foreach (var section in term.Sections)
                    {
                        var b_names = new List<string>();
                        var g_names = new List<string>();
                        b_names = boy_names.Skip(course_iteration * section_iteration * 40).Take(20).ToList();
                        g_names = girl_names.Skip(course_iteration * section_iteration * 40).Take(20).ToList();

                        foreach (var name in b_names)
                        {
                            var stu = new CreateStudentDto();
                            stu_iteration++;
                            var stuIndex = "stu-" + stu_iteration;
                            stu.Id = GuidHelper.GetStudentId(stuIndex).ToString();
                            stu.Name = name;
                            stu.CourseName = course.Name;
                            stu.CourseId = course_id.ToString();
                            stu.DOB = DateTime.Parse($"01/04/{dob_year}");
                            stu.Gender = GenderEnum.Male;
                            stu.SecName = section.Name;
                            stu.Branch = course.Branch;
                            stu.InstId = instIdGuid.ToString();
                            stu.TermName = term.Name;
                            studs.Add(stu);
                        }
                        foreach (var name in g_names)
                        {
                            var stu = new CreateStudentDto();
                            stu_iteration++;
                            var stuIndex = "stu-" + stu_iteration;
                            stu.Id = GuidHelper.GetStudentId(stuIndex).ToString();
                            stu.Name = name;
                            stu.CourseName = course.Name;
                            stu.CourseId = course_id.ToString();
                            stu.DOB = DateTime.Parse($"01/04/{dob_year}");
                            stu.Gender = GenderEnum.Female;
                            stu.SecName = section.Name;
                            stu.Branch = course.Branch;
                            stu.InstId = instIdGuid.ToString();
                            stu.TermName = term.Name;
                            studs.Add(stu);
                        }

                        section_iteration++;
                    }
                }
                course_iteration++;
            }
            var api_stu_script = GetApiScript_StudentsEtl(studs);
            foreach (var line in api_stu_script ?? new List<string>())
            {
                if (string.IsNullOrEmpty(line)) continue;
                result.Add(line);
            }
            return result;
        }
    }
    internal class StudentData
    {
        public string name { get; set; }
        public string course { get; set; }
        public string branch { get; set; }
        public string section { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string father_name { get; set; }
        public string father_email { get; set; }
        public string father_mobile { get; set; }
        public string mother_name { get; set; }
        public string mother_email { get; set; }
        public string mother_mobile { get; set; }
        public string guardian_name { get; set; }
        public string guardian_email { get; set; }
        public string guardian_mobile { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string student_id { get; set; }
    }
}