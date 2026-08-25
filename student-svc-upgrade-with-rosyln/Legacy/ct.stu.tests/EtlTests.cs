using Autofac;
using ct._Core.Policy;
using ct._Core.Repo.Resource;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using ct.stu.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Raven.Client.Documents.Linq;
using Repo.Db.Command;
using Repo.Db.Index;
using Repo.Db.Query;
using Repo.Dto;
using Repo.Resource;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ct.stu.tests
{
    [TestClass]
    public class EtlTests : TestBase
    {
        [ClassInitialize]
        public static void class_init(TestContext context)
        {
            Configure();
            ConfigureStore();
        }

        [TestMethod]
        [TestCategory("Cmd")]
        [Ignore]
        public void can_upd_stu_course()
        {
            var inst_id = id_vvs_gcs_18_19;
            var course_id = id_vvs_gcs_18_19_kg1;
            //ToDo
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .Where(o => o.CourseId == course_id)
                            .OfType<Student>()
                            .Take(256)
                            .ToList();
                foreach (var stu in stus)
                {
                    var enrl = stu.Enrollments.FirstOrDefault(o => o.InstId == inst_id && o.CourseId == course_id);
                    if (enrl == null) continue;
                    if (enrl.CourseName.StartsWith("KGI") || enrl.CourseName.StartsWith("KG I"))
                    {
                        enrl.CourseName = "KG I";
                        enrl.SectionName = string.IsNullOrEmpty(enrl.SectionName) ? "A" : enrl.SectionName;
                        enrl.SectionName = enrl.SectionName == "Blue" ? "A" :
                                          enrl.SectionName == "Green" ? "B" :
                                          enrl.SectionName == "Red" ? "C" :
                                          enrl.SectionName == "Yellow" ? "D" : enrl.SectionName;
                        session.Store(stu);
                        Console.WriteLine(stu.Id);
                    }
                }
                session.SaveChanges();
                Console.WriteLine(JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        [Ignore]
        public void can_update_rte()
        {
            var stu_ids = new List<string>();
            var lines = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "gcs_17_18_rte.csv"));
            foreach (var line in lines)
            {
                var arr = line.Split(",".ToCharArray());
                stu_ids.Add(arr[0]);
            }
            var inst_id = id_vvs_gcs_18_19;
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            using (var session = Store.OpenSession())
            {
                var dict = session.Load<Student>(stu_ids);
                foreach (var de in dict)
                {
                    var stu = de.Value;
                    var enrl = stu.Enrollments.FirstOrDefault(o => o.InstId == inst_id);
                    if (enrl == null) continue;

                    Console.WriteLine(de.Key);

                    //stu.Tags.Add("RTE");
                    //session.Store(stu);
                }
                //session.SaveChanges();
            }
        }

        [TestMethod]
        public void can_convert_csv_to_json()
        {
            var ppec_18_19_inst_id = "0c251000-1819-11e7-ff75-d17d371ad608";
            var ppec_17_18_inst_id = "0c251000-50bf-11e7-89e2-1cbdb9e7fd04";

            var course_mapping = new Dictionary<string, string>()
            {
                { "0c251020-50bf-11e7-89e2-1cbdb9e7fd04", "0c251021-1819-11e7-ff75-d17d371ad608" },// old nursery to new lkg/prep 1
                { "0c251021-50bf-11e7-89e2-1cbdb9e7fd04", "0c251022-1819-11e7-ff75-d17d371ad608" },// lkg/prep1 to ukg/prep 2
                { "0c251022-50bf-11e7-89e2-1cbdb9e7fd04", "0c251023-1819-11e7-ff75-d17d371ad608" },// ukg/prep2 to 1
                { "0c251023-50bf-11e7-89e2-1cbdb9e7fd04", "0c251024-1819-11e7-ff75-d17d371ad608" },// 1 to 2
                { "0c251024-50bf-11e7-89e2-1cbdb9e7fd04", "0c251025-1819-11e7-ff75-d17d371ad608" },// 2 to 3
                { "0c251025-50bf-11e7-89e2-1cbdb9e7fd04", "0c251026-1819-11e7-ff75-d17d371ad608" },// 3 to 4
                { "0c251026-50bf-11e7-89e2-1cbdb9e7fd04", "0c251027-1819-11e7-ff75-d17d371ad608" },// 4 to 5
                { "0c251027-50bf-11e7-89e2-1cbdb9e7fd04", "0c251028-1819-11e7-ff75-d17d371ad608" },// 5 to 6
                { "0c251028-50bf-11e7-89e2-1cbdb9e7fd04", "0c251029-1819-11e7-ff75-d17d371ad608" },// 6 to 7
                { "0c251029-50bf-11e7-89e2-1cbdb9e7fd04", "0c25102a-1819-11e7-ff75-d17d371ad608" },// 7 to 8
                { "0c25102a-50bf-11e7-89e2-1cbdb9e7fd04", "0c25102b-1819-11e7-ff75-d17d371ad608" },// 8 to 9
                { "0c25102b-50bf-11e7-89e2-1cbdb9e7fd04", "0c25102c-1819-11e7-ff75-d17d371ad608" },// 9 to 10
            };

            string file = Path.Combine(Path.GetTempPath(), "PPEC_Student.csv");
            var csv = File.ReadAllLines(file);
            var rows = csv.Skip(1);
            var result = new List<Student>();
            foreach (var row in rows)
            {
                var student = new Student();
                var values = Helper.SplitCsvRow(row);
                student.Id = values[0];

                var new_enrollment = new Enrollment();
                new_enrollment.InstId = ppec_18_19_inst_id;

                var old_enrollments = JsonConvert.DeserializeObject<List<Enrollment>>(CsvService.Unescape(values[1]));
                var enrl = old_enrollments.Where(en => en.InstId == ppec_17_18_inst_id && en.Status == EnrollmentStatusEnum.Active).FirstOrDefault();

                var new_course_id = course_mapping.ContainsKey(enrl.CourseId) ? course_mapping[enrl.CourseId] : "";
                if (string.IsNullOrEmpty(new_course_id)) continue; //Xth students

                student.EnrolInInstitute(ppec_18_19_inst_id, enrl.CourseName, new_enrollment.CourseId, enrl.TermName, enrl.SectionName, enrl.Branch, enrl.StudentId, enrl.DOA);
                student.Enrollments.Distinct();

                student.StudentId = values[2];
                student.Status = (StudentStatusEnum)Enum.Parse(typeof(StudentStatusEnum), values[4]);
                student.Father.Contacts = JsonConvert.DeserializeObject<List<Contact>>(CsvService.Unescape(values[27]));
                student.Father.Id = values[33];
                student.Mother.Id = values[69];
                student.Guardian.Id = values[105];
                student.FirstName = values[117];
                student.MiddleName = values[118];
                student.LastName = values[119];
                student.Name = values[120];
                student.Gender = (GenderEnum)Enum.Parse(typeof(GenderEnum), values[122]);
                student.DOB = DateTime.Parse(values[123]);

                result.Add(student);
            }
            var json = JsonConvert.SerializeObject(result);
            string file_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            File.WriteAllText(Path.Combine(file_path, "PPEC_Promoted_Students.json"), json);
        }

        [TestMethod]
        public void can_convert_new_student_csv_to_json()
        {
            string file = Path.Combine(Path.GetTempPath(), "PPEC_New_Students_18_19.csv");
            var csv = File.ReadAllLines(file);
            var rows = csv.Skip(1);
            var result = new List<Student>();
            foreach (var row in rows)
            {
                var student = new Student();
                var values = Helper.SplitCsvRow(row);
                student.Name = values[0];
                student.DOB = DateTime.Parse(values[1]);
                student.EnrolInInstitute("0c251000-50bf-11e7-ff75-d17d371ad608", values[5], values[7], "Annual", values[6], "", "", new DateTime());
                student.Father.Mobile = values[11];

                result.Add(student);
            }
            var json = JsonConvert.SerializeObject(result);
            string file_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            File.WriteAllText(Path.Combine(file_path, "PPEC_New_Student_18_19.json"), json);
        }

        [TestMethod]
        public void can_create_ppec_Promoted_student_etl()
        {
            // read contents from file
            var json = File.ReadAllText(Path.Combine(Path.GetTempPath(), "PPEC_Promoted_Students.json"));
            var students = JsonConvert.DeserializeObject<List<Student>>(json);
            // convert to etl script format
            var sb = new StringBuilder();
            var prefix = @"post /api/stu/student";
            var prefix_father_contact = @"post /api/stu/student/updateFatherDetails";
            string file_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter output_file = new StreamWriter(Path.Combine(file_path, "etl_scripts/PPEC_Promoted_Students_Etl.txt")))
            {
                using (StreamWriter stu_mobile_etl = new StreamWriter(Path.Combine(file_path, "etl_scripts/PPEC_Father_Contact_Etl.txt")))
                {
                    foreach (var stu in students)
                    {
                        var create_payload = new CreateStudentDto();
                        var father_details_payload = new UpdateFatherDetailsStudentDto();
                        create_payload.AadharNumber = stu.AadharNumber;
                        create_payload.AdmissionNumber = stu.AadharNumber;
                        create_payload.Branch = stu.Enrollments[0].Branch;
                        create_payload.CourseId = stu.Enrollments[0].CourseId;
                        create_payload.CourseName = stu.Enrollments[0].CourseName;
                        create_payload.DOA = stu.Enrollments[0].DOA;
                        create_payload.DOB = stu.DOB;
                        create_payload.Email = stu.Email;
                        create_payload.Gender = stu.Gender;
                        create_payload.Mobile = stu.Mobile;
                        create_payload.Name = stu.Name;
                        create_payload.SecName = stu.Enrollments[0].SectionName;
                        create_payload.StudentId = stu.Enrollments[0].StudentId;
                        create_payload.TermName = stu.Enrollments[0].TermName;
                        create_payload.InstId = stu.Enrollments[0].InstId;
                        create_payload.Id = stu.Id;

                        var mobile_contact = stu.Father.Contacts.Where(c => c.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();

                        father_details_payload.Id = stu.Id;
                        father_details_payload.Mobile = mobile_contact.Info;
                        father_details_payload.Name = stu.Father.Name;
                        father_details_payload.Email = stu.Father.Email;

                        var api_stu_create_post = $"{prefix} {JsonConvert.SerializeObject(create_payload)}";
                        var api_father_mobile_post = $"{prefix_father_contact} {JsonConvert.SerializeObject(father_details_payload)}";

                        output_file.WriteLine(api_stu_create_post);
                        stu_mobile_etl.WriteLine(api_father_mobile_post);
                    }
                    output_file.WriteLine(sb.ToString());
                }
            }
        }

        [TestMethod]
        public void can_create_ppec_new_student_etl()
        {
            // read contents from file
            var json = File.ReadAllText(Path.Combine(Path.GetTempPath(), "PPEC_New_Students_18_19.json"));
            var students = JsonConvert.DeserializeObject<List<Student>>(json);
            // convert to etl script format
            var sb = new StringBuilder();
            var prefix = @"post /api/stu/student";
            string file_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter output_file = new StreamWriter(Path.Combine(file_path, "etl_scripts/PPEC_New_Student_Etl.txt")))
            {
                foreach (var stu in students)
                {
                    var payload = new CreateStudentDto();
                    payload.AadharNumber = stu.AadharNumber;
                    payload.AdmissionNumber = stu.AadharNumber;
                    payload.Branch = stu.Enrollments[0].Branch;
                    payload.CourseId = stu.Enrollments[0].CourseId;
                    payload.CourseName = stu.Enrollments[0].CourseName;
                    payload.DOA = stu.Enrollments[0].DOA;
                    payload.DOB = stu.DOB;
                    payload.Email = stu.Email;
                    payload.Gender = stu.Gender;
                    payload.Mobile = stu.Mobile;
                    payload.Name = stu.Name;
                    payload.SecName = stu.Enrollments[0].SectionName;
                    payload.StudentId = stu.Enrollments[0].StudentId;
                    payload.TermName = stu.Enrollments[0].TermName;
                    payload.InstId = stu.Enrollments[0].InstId;

                    var api_post = $"{prefix} {JsonConvert.SerializeObject(payload)}";
                    sb.Append(api_post);
                    sb.Append(Environment.NewLine);
                }
                output_file.WriteLine(sb.ToString());
            }
        }

        [TestMethod]
        public void can_create_script_for_student_promotion()
        {
            var base_inst_id = id_vvs_gcs_23_24;
            var short_name = "VVSGCS";
            var target_year = "2425";
            var env = "prod";

            var uc = new UserContext(null)
            {
                InstId = base_inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var _awsService = new AWSService(Config, uc);
                var rmq = new RabbitMQService(Config);
                var _studentQry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                EtlService etlService = new EtlService(uc, _awsService, _studentQry);
                var sb = new StringBuilder();

                var api_inst_script = etlService.GetApiScript_PromoteStudents(env, short_name.ToLower(), target_year, base_inst_id);
                foreach (var line in api_inst_script ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    sb.Append(line);
                    sb.Append(Environment.NewLine);
                }
                string script_file_path = etlService.GetEtlFileName(short_name, target_year, env);
                //etlService.WriteScriptToS3(script_file_path, sb.ToString());
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "students-7-8.etl"), sb.ToString());
            }
        }

        [TestMethod]
        public void can_create_script_to_import_new_students_from_student_data_template()
        {
            var inst_id = id_vvs_bms_26_27;
            var short_name = "VVSBMS";

            var target_year = "2627";
            var env = "prod";

            var filename = "X.csv";
            var course_id = "1a6a7032-2627-45ec-9500-f37f54f82d5f";
            var course_name = "X";
            var term_name = "Annual";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var _awsService = new AWSService(Config, uc);
                var rmq = new RabbitMQService(Config);
                var _studentCmd = new StudentCmd(uc, rmq, policyEngineProvider: pep);
                var _studentQry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                EtlService etlService = new EtlService(uc, _awsService, _studentQry, _studentCmd);
                var sb = new StringBuilder();

                var api_inst_script = etlService.GetApiScript_NewStudentsImport(filename, course_id, term_name, inst_id);
                foreach (var line in api_inst_script ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    sb.Append(line);
                    sb.Append(Environment.NewLine);
                }
                string script_file_path = etlService.GetEtlFileName_NewStudents(short_name, target_year, course_name, env);
                //etlService.WriteScriptToS3(script_file_path, sb.ToString());
             File.WriteAllText(Path.Combine(Path.GetTempPath(), "prod_04_vvsbms_X_students.etl"), sb.ToString());
            }
        }



        [TestMethod]
        public void generate_student_names()
        {

            Random rnd = new Random();
            var inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";
            var acad_year_start = 2020;
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var _awsService = new AWSService(Config, uc);
                var rmq = new RabbitMQService(Config);
                var _studentCmd = new StudentCmd(uc, rmq, policyEngineProvider: pep);
                var _studentQry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                EtlService etlService = new EtlService(uc, _awsService, _studentQry, _studentCmd);
                var sb = new StringBuilder();

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

                var demo_institute_id = "62f6c897-4d42-4420-9431-6d64d269b54e";

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

                var course_list = new List<CourseAge>()
            {
                new CourseAge{ CourseName = "Playhome", Age = 3, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "KG I", Age = 4, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "KG II", Age = 5, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "I", Age = 6, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "II", Age = 7, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "III", Age = 8, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "IV", Age = 9, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "V", Age = 10, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "VI", Age = 11, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                new CourseAge{ CourseName = "VII", Age = 12, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
            };
                var studs = new List<CreateStudentDto>();

                var course_iteration = 1;
                var section_iteration = 0;
                foreach (var crs in course_list)
                {
                    var dob_year = acad_year_start - crs.Age;
                    var guid_inst_id = GuidHelper.Parse(demo_institute_id.Trim());

                    foreach (var sec in crs.Sections)
                    {
                        var b_names = new List<string>();
                        var g_names = new List<string>();
                        b_names = boy_names.Skip(course_iteration * section_iteration * 40).Take(20).ToList();
                        g_names = girl_names.Skip(course_iteration * section_iteration * 40).Take(20).ToList();

                        foreach (var name in b_names)
                        {
                            var stu = new CreateStudentDto();
                            stu.Id = Guid.NewGuid().ToString();
                            stu.Name = name;
                            stu.CourseName = crs.CourseName;
                            stu.CourseId = GuidHelper.GetCourseId(guid_inst_id, crs.CourseName.Trim()).ToString();
                            stu.DOB = DateTime.Parse($"01/04/{dob_year}");
                            stu.Gender = GenderEnum.Male;
                            stu.SecName = sec.Name;
                            stu.Branch = crs.Branch;
                            stu.InstId = demo_institute_id;
                            stu.TermName = crs.Term;
                            studs.Add(stu);
                        }
                        foreach (var name in g_names)
                        {
                            var stu = new CreateStudentDto();
                            stu.Id = Guid.NewGuid().ToString();
                            stu.Name = name;
                            stu.CourseName = crs.CourseName;
                            stu.CourseId = GuidHelper.GetCourseId(guid_inst_id, crs.CourseName.Trim()).ToString();
                            stu.DOB = DateTime.Parse($"01/04/{dob_year}");
                            stu.Gender = GenderEnum.Female;
                            stu.SecName = sec.Name;
                            stu.Branch = crs.Branch;
                            stu.InstId = demo_institute_id;
                            stu.TermName = crs.Term;
                            studs.Add(stu);
                        }
                        section_iteration++;
                    }
                    course_iteration++;
                }

                var api_inst_script = etlService.GetApiScript_StudentsEtl(studs);
                foreach (var line in api_inst_script ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    sb.Append(line);
                    sb.Append(Environment.NewLine);
                }
                string script_file_path = etlService.GetEtlFileName_DemoStudents(short_name, acad_year_start.ToString());
                etlService.WriteScriptToS3(script_file_path, sb.ToString());
            }
        }

        public int[] Shuffle(int n)
        {
            var randomGenerator = new Random();
            return Enumerable.Range(0, n).OrderBy(x => randomGenerator.Next()).ToArray();
        }

        [TestMethod]
        public void can_setup_demo_students()
        {
            var etl_lines = new List<string>();

            // uc
            var base_inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                InstId = base_inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            EtlService _etlService = new EtlService(uc);

            var p_orgShortName = "BITLOK";
            // setup students for BTLSCH
            var instShortName = "BTLSCH";
            var instAcadYear = "2021";
            var instIdGuid = GuidHelper.GetInstId($"{p_orgShortName}.{instShortName}", instAcadYear);
            var btlsch_json = File.ReadAllText(Path.Combine(Path.GetTempPath(), "BTLSCH_Courses.json"));
            var btlsch_courses = JsonConvert.DeserializeObject<List<Course>>(btlsch_json);
            var etl_student_lines = _etlService.get_etl_students(btlsch_courses, instIdGuid);
            foreach (var line in etl_student_lines ?? new List<string>()) { etl_lines.Add(line); }

            // setup students for BTLCOL
            var btlcolInstShortName = "BTLCOL";
            var btlcolInstAcadYear = "2021";
            var btlcolInstIdGuid = GuidHelper.GetInstId($"{p_orgShortName}.{btlcolInstShortName}", btlcolInstAcadYear);
            var btlcol_json = File.ReadAllText(Path.Combine(Path.GetTempPath(), "BTLCOL_Courses.json"));
            var btlcol_courses = JsonConvert.DeserializeObject<List<Course>>(btlcol_json);
            var btlcol_etl_student_lines = _etlService.get_etl_students(btlcol_courses, btlcolInstIdGuid);
            foreach (var line in btlcol_etl_student_lines ?? new List<string>()) { etl_lines.Add(line); }

            // Build script
            var sb = new StringBuilder();
            foreach (var line in etl_lines ?? new List<string>())
            {
                if (string.IsNullOrEmpty(line)) continue;
                sb.Append(line);
                sb.Append(Environment.NewLine);
            }
            string file_path = Path.Combine(Path.GetTempPath(), $"{p_orgShortName.ToLower()}_students.etl");
            using (StreamWriter output_file = new StreamWriter(file_path))
            {
                output_file.WriteLine(sb.ToString());
            }
        }

        [TestMethod]
        public void can_update_student_mobile()
        {
            var base_inst_id = id_vvs_sphs_20_21;
            var short_name = "VVSPHS";
            var target_year = "2021";
            var env = "prod";

            var uc = new UserContext(null)
            {
                InstId = base_inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var _awsService = new AWSService(Config, uc);
                var rmq = new RabbitMQService(Config);
                var _studentQry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                EtlService etlService = new EtlService(uc, _awsService, _studentQry);
                var sb = new StringBuilder();

                var result = new List<SetPrimaryMobileDto>();

                string file = Path.Combine(Path.GetTempPath(), "SPHS-student-contact.csv");
                var csv = File.ReadAllLines(file);
                var rows = csv.Skip(1);
                foreach (var row in rows)
                {
                    var stu = new SetPrimaryMobileDto();
                    var values = Helper.SplitCsvRow(row);
                    stu.Id = values[0];
                    stu.Mobile = values[1].ToString();
                    result.Add(stu);
                }

                var api_inst_script = etlService.GetApiScript_UpdateStudentMobile(result);
                foreach (var line in api_inst_script ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    sb.Append(line);
                    sb.Append(Environment.NewLine);
                }
                string script_file_path = etlService.GetEtlFileName(short_name, target_year, env);
                etlService.WriteScriptToS3(script_file_path, sb.ToString());
            }
        }
        [TestMethod]
        public void can_upd_stu_virtual_id()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .OfType<Student>()
                            .Take(2048)
                            .ToList();
                foreach (var stu in stus)
                {
                    session.Store(stu);
                }
                session.SaveChanges();
            }
        }

        [TestMethod]
        public void can_get_stu_data()
        {
            var inst_id = id_vvs_sppu_21_22;
            var short_name = "VVSSPU";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                            .Where(o => o.CourseId == "1a6a5020-2122-45ec-9500-f37f54f82d5f" && o.SectionName == "A")
                            .OfType<Student>()
                            .ToList();

                var result = new List<StuVirtualId>();
                foreach (var stu in stus)
                {
                    var s = new StuVirtualId();
                    s.Id = stu.Id;
                    s.Name = stu.Name;
                    //s.LastName = stu.LastName;
                    s.Email = stu.VirtualId;
                    s.Mobile = stu.Mobile;
                    s.Password = stu.FirstName + stu.Id.Substring(stu.Id.Length - 4);
                    // s.RecoveryEmail = string.IsNullOrEmpty(stu.Father.Email) ? stu.Mother.Email : stu.Father.Email;
                    // s.RecoveryMobile = string.IsNullOrEmpty(stu.Father.Mobile) ? stu.Mother.Mobile : stu.Father.Mobile;
                    //s.Mobiles = stu.MobileCSV.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
                    result.Add(s);
                }

                File.WriteAllText(Path.Combine(Path.GetTempPath(), "pcmb-a.json"), JsonConvert.SerializeObject(result));
            }
        }

        [TestMethod]
        public void can_create_etl_gcs_opening_balance()
        {
            var base_inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";
            var target_year = "2223";
            var env = "prod";
            var uc = new UserContext(null)
            {
                InstId = base_inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var _awsService = new AWSService(Config, uc);
                EtlService etlService = new EtlService(uc, _awsService);
                var sb = new StringBuilder();

                var json_20_21 = File.ReadAllText(Path.Combine(Path.GetTempPath(), "fee-txn-2021.json"));
                var txn_20_21 = JsonConvert.DeserializeObject<List<FeeRecords>>(json_20_21);
                txn_20_21 = txn_20_21.Where(o => o.cl_bal != 0).ToList();

                var final_list = new List<FeesReceivableDto>();
                foreach (var tx in txn_20_21)
                {
                    var dto = new FeesReceivableDto();
                    dto.Id = tx.student_id;
                    dto.InstId = base_inst_id;
                    dto.Amount = tx.cl_bal;
                    final_list.Add(dto);
                }

                var api_inst_script = etlService.GetApiScript_OpeningBalance(final_list);
                foreach (var line in api_inst_script ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    sb.Append(line);
                    sb.Append(Environment.NewLine);
                }
                string script_file_path = etlService.GetEtlFileName_Opening_balance(short_name, target_year, env);
                // etlService.WriteScriptToS3(script_file_path, sb.ToString());
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "opening_bal_21_22.txt"), sb.ToString());
            }
        }

        public class FeeRecords
        {
            public string inst_name { get; set; }
            public string course { get; set; }
            public string section { get; set; }
            public string student_name { get; set; }
            public string student_ref_id { get; set; }
            public string status { get; set; }
            public decimal op_bal { get; set; }
            public decimal fees_due { get; set; }
            public decimal cep { get; set; }
            public decimal sdf { get; set; }
            public decimal fees_tution { get; set; }
            public decimal fees_others { get; set; }
            public decimal fees_discount { get; set; }
            public decimal fees_adj { get; set; }
            public int cl_bal { get; set; }
            public string inst_id { get; set; }
            public string student_id { get; set; }
        }

        public class StuVirtualId
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            // public string LastName { get; set; }
            // public string RecoveryEmail { get; set; }
            // public string RecoveryMobile { get; set; }
            public string Mobile { get; set; }
            public string Password { get; set; }
        }

        [TestMethod]
        public void generate_student_etl()
        {
            var inst_id = "67dd9f97-c53d-5681-91fc-5ae39d60d62b";
            var short_name = "BTLSCH";
            var acad_year = 2024;
            Random rnd = new Random();
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var _awsService = new AWSService(Config, uc);
                var rmq = new RabbitMQService(Config);
                var _studentCmd = new StudentCmd(uc, rmq, policyEngineProvider: pep);
                var _studentQry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                EtlService etlService = new EtlService(uc, _awsService, _studentQry, _studentCmd);
                var sb = new StringBuilder();

                var first_names = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "firstname.csv"));
                var last_names_list = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "lastname.csv"));
                var f_list = Helper.CsvToList<FirstName>(first_names);
                var f_list_string = JsonConvert.SerializeObject(f_list);
                var f_json = JsonConvert.DeserializeObject<List<FirstName>>(f_list_string);
                var boys = f_json.Where(o => o.gender == "Male").ToList();
                var girls = f_json.Where(o => o.gender == "Female").ToList();

                var boys_index = boys.OrderBy(item => rnd.Next());
                var girls_index = girls.OrderBy(item => rnd.Next());
                var last_names = last_names_list.ToList();
                last_names = last_names.OrderBy(item => rnd.Next()).ToList();
                var ln_index = 1;
                var boy_names = new List<string>();
                var girl_names = new List<string>();

                foreach (var i in boys_index)
                {
                    var b_list = boys.ToList();
                    var item = i.first_name;
                    if (item != last_names[ln_index])
                    {
                        string name = item + ' ' + last_names[ln_index];
                        if ((last_names.Count() - 1) != ln_index) ln_index++;
                        else ln_index = 1;
                        boy_names.Add(name);
                    }
                }
                foreach (var i in girls_index)
                {
                    var b_list = girls.ToList();
                    var item = i.first_name;
                    if (item != last_names[ln_index])
                    {
                        string name = item + ' ' + last_names[ln_index];
                        if ((last_names.Count() - 1) != ln_index) ln_index++;
                        else ln_index = 1;
                        girl_names.Add(name);
                    }
                }

                var course_list = new List<CourseAge>()
                {
                    new CourseAge{ Id = "88ba618f-b809-4eb8-b98a-8d44b25520ec", CourseName = "I", Age = 6, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ Id = "9353ae89-087e-4e15-bc33-8e2126e030bc", CourseName = "II", Age = 7, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ Id = "b89aefe5-0f25-408b-9eb7-9c8b47ecb393", CourseName = "III", Age = 8, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ Id = "f4a5049e-c422-45d1-85db-410d4438dee2", CourseName = "IV", Age = 9, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    // new CourseAge{ CourseName = "V", Age = 10, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    // new CourseAge{ CourseName = "VI", Age = 11, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    // new CourseAge{ CourseName = "VII", Age = 12, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                };

                var studs = new List<CreateStudentDto>();

                foreach (var course in course_list)
                {
                    DateTime startDate = new DateTime(acad_year - course.Age, 1, 1);
                    DateTime endDate = new DateTime(acad_year - course.Age, 12, 31);
                    TimeSpan timeSpan = endDate - startDate;
                    var randomTest = new Random();
                    var dobs = new List<string>();

                    for (var i = 0; i < 20; i++)
                    {
                        TimeSpan newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);
                        DateTime newDate = startDate + newSpan;
                        dobs.Add(newDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
                    }


                    foreach (var section in course.Sections)
                    {
                        var bys = boy_names.OrderBy(item => rnd.Next());
                        var gls = girl_names.OrderBy(item => rnd.Next());

                        var s_boys = bys.Take(10).ToList();
                        var s_girls = gls.Take(10).ToList();
                        var s_dobs = dobs.Take(20).ToList();

                        var dob_idx = 0;

                        foreach (var name in s_boys)
                        {
                            var stu = new CreateStudentDto();
                            stu.Id = Guid.NewGuid().ToString();
                            stu.Name = name;
                            stu.CourseName = course.CourseName;
                            stu.CourseId = course.Id;
                            stu.DOB = DateTime.Parse(s_dobs[dob_idx]);
                            stu.Gender = GenderEnum.Male;
                            stu.SecName = section.Name;
                            stu.Branch = course.Branch;
                            stu.InstId = inst_id;
                            stu.TermName = "Annual";
                            studs.Add(stu);
                            dob_idx++;
                        }
                        foreach (var name in s_girls)
                        {
                            var stu = new CreateStudentDto();
                            stu.Id = Guid.NewGuid().ToString();
                            stu.Name = name;
                            stu.CourseName = course.CourseName;
                            stu.CourseId = course.Id;
                            stu.DOB = DateTime.Parse(s_dobs[dob_idx]);
                            stu.Gender = GenderEnum.Female;
                            stu.SecName = section.Name;
                            stu.Branch = course.Branch;
                            stu.InstId = inst_id;
                            stu.TermName = course.Term;
                            studs.Add(stu);
                            dob_idx++;
                        }

                    }
                }
                var api_inst_script = etlService.GetApiScript_StudentsEtlJsonl(studs);
                foreach (var line in api_inst_script ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    sb.Append(line);
                    sb.Append(Environment.NewLine);
                }
                string script_file_path = etlService.GetEtlFileName_DemoStudents(short_name, acad_year.ToString());
                //  etlService.WriteScriptToS3(script_file_path, sb.ToString());
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "04_students.etl"), sb.ToString());

            }
        }

        [TestMethod]
        public void get_random_date()
        {
            var acad_year = 2023;
            var course_list = new List<CourseAge>()
                {
                    new CourseAge{ CourseName = "I", Age = 6, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ CourseName = "II", Age = 7, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ CourseName = "III", Age = 8, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ CourseName = "IV", Age = 9, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ CourseName = "V", Age = 10, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ CourseName = "VI", Age = 11, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                    new CourseAge{ CourseName = "VII", Age = 12, Sections = new List<EtlSection>(){new EtlSection() {Name = "A"}, {new EtlSection() {Name = "B"}}}},
                };

            foreach (var course in course_list)
            {
                DateTime startDate = new DateTime(acad_year - course.Age, 1, 1);
                DateTime endDate = new DateTime(acad_year - course.Age, 12, 31);
                TimeSpan timeSpan = endDate - startDate;
                var randomTest = new Random();

                for (var i = 0; i < 20; i++)
                {
                    TimeSpan newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);
                    DateTime newDate = startDate + newSpan;

                    Console.WriteLine(course.CourseName);
                    Console.WriteLine(newDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
                }
            }
        }

        [TestMethod]
        public void get_students_for_fee_txns()
        {
            var inst_id = id_btl_col_23_24;
            var short_name = "BTLCOL";
            var acad_year = 2023;
            Random rnd = new Random();
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                BoundByInst = true,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var _awsService = new AWSService(Config, uc);
                var rmq = new RabbitMQService(Config);
                var _studentQry = new StudentQry(uc, rmq, policyEngineProvider: pep);

                var course_ids = new List<string>() { "5005f564-9db1-497b-b6ed-bd7e03f026da", "4f08c477-5f48-4a10-9579-5c98ac93b0af", "8bba5904-13ca-4c72-babf-e39aa71fa4e7", "b986ae10-46a9-43fa-898a-d9192fb74f60", "8d5c97b1-0bde-4176-b5b7-30e13ccbbf5a", "8bf48118-025e-4403-8496-64aa6d075bb6", "58ffbe01-8fd1-4184-bc33-e2c00310e1cf" };

                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .Where(o => o.CourseId.In(course_ids));
                var stus = new List<string>();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                    }
                }
            }
        }
    }
}
