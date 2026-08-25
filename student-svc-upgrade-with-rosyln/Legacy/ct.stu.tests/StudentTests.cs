using Autofac;
using ct._Core.Policy;
using ct._Core.Repo.Resource;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Raven.Client.Documents;
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
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace ct.stu.tests
{
    [TestClass]
    public class StudentTests : TestBase
    {
        private static TestContext _context;
        [ClassInitialize]
        public static void class_init(TestContext context)
        {
            Console.WriteLine(Path.GetTempPath());
            _context = context;
            Configure();
            ConfigureStore();
        }

        [TestInitialize]
        public void test_init()
        {
        }

        [TestMethod]
        public void can_run_settings()
        {
            //TBD: Should be run from console => dotnet test --settings test.runsettings etc.
            Console.WriteLine(_context.Properties["env"]);
        }

        [TestMethod]
        [TestCategory("Cmd")]
        public void can_create_student()
        {
            var inst_id = id_btl_col_19_20;
            var short_name = "BTLCOL";
            DateTime dob = DateTime.Parse("02/05/2002");
            DateTime doa = DateTime.MinValue;
            var courseId = "d91ae84b-1920-47fd-80c4-675d206eca75";
            var courseName = "PUC I PCMC";
            var branch = "PCMC";
            var student_name = "Mohit N";
            var section_name = "A";
            var term = "Annual";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var cmd = new StudentCmd(uc, null, null);
                //cmd.Create(null,null,null,"PUC I","e2e70d87-af8d-4aa4-ac89-4e4d9ca83332","2008/6/13","Fri Jun 13 2008",null,"Male",null,"Saketh Datt","A","","Annual","","Wed May 15 2024","Wed May 15 2024","4589","Prakash Datt","p@abc.com","1111111111","Software Engineer","Accenture","Sunitha Datt","s@abc.com","2222222222","Software Suport Engineer","TCS","101","HSR Layout","Bangalore","560102","Karnataka","India","","");
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_student()
        {
            var session = Store.OpenSession();
            var res = session.Query<Student>();
            Assert.IsNotNull(res);
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_course()
        {
            var inst_id = "0c250000-50bf-11e7-89e2-1cbdb9e7fd04";
            //ToDo
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var data = session
                            .Advanced.RawQuery<Course>($"from Courses where InstId='{inst_id}'")
                            .ToList();

                Console.WriteLine(JsonConvert.SerializeObject(data));
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_student()
        {
            var inst_id = id_vvs_gcs_19_20;
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
                        .Where(o => o.Id == "01702e5d-3930-4d19-bbdb-6feaf020a86d")
                        .OfType<Student>()
                        // .ProjectInto<Student_Inst_Index.Result>()
                        .ToList();
                Console.WriteLine(JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_one_student()
        {
            var inst_id = id_vvs_sphs_18_19;
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                        .Where(o => o.InstId == inst_id)
                        .ProjectInto<Student_Inst_Index.Result>()
                        .Take(1)
                        .ToList();
                Console.WriteLine(JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_stu_for_facet()
        {
            //var inst_id = "0c250000-50bf-11e7-89e2-1cbdb9e7fd04";
            var inst_id = id_vvs_gcs_18_19;
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId == inst_id;
            Expression<Func<Student_Inst_Index.Result, bool>> filterByEnrollmentStatus = o => o.EnrollmentStatus == EnrollmentStatusEnum.Active;
            //ToDo
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            using (var _session = Store.OpenSession())
            {
                //qry
                var qry = _session
                                .Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                .Where(o => o.InstId == inst_id)
                                .Where(filterByEnrollmentStatus)
                                .ProjectInto<Student_Inst_Index.Result>();
                var stus = new List<Student_Inst_Index.Result>();
                using (var enumerator = _session.Advanced.Stream(qry))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        stus.Add(stu);
                    }
                }

                var group = stus
                              .OrderBy(o => o.CourseSortIndex).ThenBy(o => o.SectionName)
                              .GroupBy(o => new { o.CourseId, o.CourseNameAndBranch })
                              .Select(g => new
                              {
                                  CourseId = g.Key.CourseId,
                                  CourseName = g.Key.CourseNameAndBranch,
                                  Sections = g.ToList()
                                               .GroupBy(s => s.SectionName)
                                               .Select(x => new { Section = x.Key, Students = x.ToList() })

                              });
                var courses = group.ToList();

                var result = new List<Facet>();


                var parent_facet = new Facet() { ElemId = "Parent", Name = "Parent", Edit = false };
                var student_facet = new Facet() { ElemId = "Student", Name = "Student", Edit = false };
                foreach (var c in courses)
                {
                    var course_facet = new Facet() { ElemId = c.CourseId, Name = c.CourseName, Edit = false };
                    foreach (var sec in c.Sections)
                    {
                        var sec_facet = new Facet() { ElemId = sec.Section, Name = sec.Section, Edit = false };
                        var stu_list = sec.Students.OrderBy(o => o.Name).Select(o => new { o.Id, o.Name }).ToList();
                        foreach (var stu in stu_list)
                        {
                            var stu_facet = new Facet() { ElemId = stu.Id, Name = stu.Name, Edit = false };
                            sec_facet.Children.Add(stu_facet);
                        }
                        course_facet.Children.Add(sec_facet);
                    }
                    parent_facet.Children.Add(course_facet);
                    student_facet.Children.Add(course_facet);
                }

                result.Add(parent_facet);
                result.Add(student_facet);
                Console.WriteLine(JsonConvert.SerializeObject(result));
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_mobiles()
        {
            var inst_id = id_ppec_18_19;
            var uc = new UserContext(null) { InstId = inst_id, ShortName = "PPECKN" };
            RecipientPostData receipient_post_data = get_recipient_post_data();
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var qry = new StudentQry(uc, null, pep);
                var cmd = new StudentCmd(uc, rmq, AutofacContainer.Resolve<IPolicyEngineProvider>());
                var data = cmd.GetSMSRecipients(receipient_post_data);
                Console.WriteLine(JsonConvert.SerializeObject(data));
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        [Ignore]
        public void can_send_sms()
        {
            var inst_id = id_ppec_18_19;
            var uc = new UserContext(null) { InstId = inst_id, ShortName = "PPECKN" };
            RecipientPostData receipient_post_data = get_recipient_post_data();
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var qry = new StudentQry(uc, rmq, pep);
                var cmd = new StudentCmd(uc, rmq, AutofacContainer.Resolve<IPolicyEngineProvider>());
                var data = cmd.GetSMSRecipients(receipient_post_data);
                var mobiles = data.Select(o => o.mobile).Distinct().ToList();
                Console.WriteLine($"Sending message: {receipient_post_data.Text} to: {string.Join(',', mobiles)} ");
                // qry.SendSMS(mobiles, receipient_post_data.Text, senderId: "BITLOK");
            }
        }

        [TestMethod]
        public void can_get_student_coursewise_count()
        {
            var inst_id = id_btl_col_18_19;
            var uc = new UserContext(null) { InstId = inst_id };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var qry = new StudentQry(uc, rmq);
                var data = session
                            .Query<Student_Coursewise_Count.Result, Student_Coursewise_Count>()
                            .Where(o => o.InstId == inst_id)
                            .ToList();
                var file_path = Path.Combine(Path.GetTempPath(), $"can_get_student_coursewise_count.txt");
                Console.WriteLine(file_path);
                File.WriteAllText(file_path, JsonConvert.SerializeObject(data));
            }
        }

        [TestMethod]
        public void can_get_policies()
        {
            var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
            var pe = pep.For("VVSGCS");
            Console.WriteLine(pe.CoursePrefix(""));
            pe = pep.For("");
            Console.WriteLine(pe.CoursePrefix(""));
            pe = pep.For("VVSPHS");
            Console.WriteLine(pe.CoursePrefix(""));
        }
        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_stu_prefix()
        {
            var doa = DateTime.Parse("2018-05-01");
            var inst_id = id_btl_col_18_19; var sms_sender_id = "BITLOK";
            //var inst_id = id_vvs_gcs_18_19; var sms_sender_id = "VVSGCS";
            // var inst_id = id_vvs_sphs_18_19; var sms_sender_id = "VVSPHS";
            var course_name = "PUC I";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                SmsSenderId = sms_sender_id //Lets determine the Policy
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var cmd = new StudentCmd(uc, rmq, AutofacContainer.Resolve<IPolicyEngineProvider>());
                var result = cmd.GetNextStudentId(DateTime.MinValue, course_name);
                Console.WriteLine(result);
                result = cmd.GetNextStudentId(doa, course_name);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), result);
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_stu_prefix_for_id()
        {
            var doa = DateTime.Parse("2017-05-01");
            //var inst_id = id_btl_col_17_18; var sms_sender_id = "BITLOK";
            var inst_id = id_vvs_gcs_18_19; var sms_sender_id = "VVSGCS";
            //var inst_id = id_vvs_sphs_18_19; var sms_sender_id = "VVSPHS";
            var course_name = "Playhome ";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                SmsSenderId = sms_sender_id //Lets determine the Policy
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var cmd = new StudentCmd(uc, rmq, AutofacContainer.Resolve<IPolicyEngineProvider>());
                var result = cmd.GetNextStudentId(DateTime.MinValue, course_name);
                Console.WriteLine(result);
                //result = cmd.GetNextStudentId(doa, course_name);
                //Console.WriteLine(result);
            }
        }

        [TestMethod]
        public void can_get_yy()
        {
            Console.WriteLine(DateTime.Now.ToString("yy"));
        }
        [TestMethod]
        public void can_pad_int()
        {
            var i = 2;
            Console.WriteLine((++i).ToString("D3"));
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_gcs_student()
        {
            var inst_id = id_vvs_gcs_18_19;
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            var sb = new StringBuilder();
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var qry = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                        .Where(o => o.InstId == inst_id)
                        .Where(o => string.IsNullOrEmpty(o.StudentId) || o.StudentId == "NA")
                        .ProjectInto<Student_Inst_Index.Result>();
                using (var enumerator = session.Advanced.Stream(qry))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var student_id = stu.StudentId == stu.Id ? "" : stu.StudentId;
                        sb.AppendLine($"{stu.Id},{stu.Name},{student_id}");
                    }
                }
            }
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "gcs_ct_stu_2.csv"), sb.ToString());
        }
        [TestMethod]
        [TestCategory("Qry")]
        public void can_set_student_id_gcs_student()
        {
            var stu_ct_lines = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "gcs_ct_stu.csv"));
            var stu_eems_lines = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "gcs_eems_stu.csv"));

            var stu_ct_list = new List<GcsStuDto>();
            var stu_eems_list = new List<GcsStuDto>();

            foreach (var line in stu_ct_lines)
            {
                var arr = line.Split(",".ToCharArray());
                var id = arr[0];
                var name = arr[1];
                name = name.Replace(".", " ");
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                name = regex.Replace(name, " ");
                stu_ct_list.Add(new GcsStuDto()
                {
                    Id = id,
                    Name = name
                });
            }

            foreach (var line in stu_eems_lines)
            {
                var arr = line.Split(",".ToCharArray());
                var stu_id = arr[0];
                var name = arr[1];
                name = name.Replace(".", " ");
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                name = regex.Replace(name, " ");
                stu_eems_list.Add(new GcsStuDto()
                {
                    StudentId = stu_id,
                    Name = name
                });
            }
            var sb = new StringBuilder();
            foreach (var stu in stu_ct_list)
            {
                var stu_eems = stu_eems_list.Where(o => o.Name.Equals(stu.Name, StringComparison.CurrentCultureIgnoreCase));
                if (stu_eems.Count() == 1)
                {
                    sb.AppendLine($"{stu.Id},{stu.Name},{stu_eems.First().StudentId}");
                }
            }
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "gcs_ct_eems_stu.csv"), sb.ToString());
        }

        [TestMethod]
        public void can_update_stu_id()
        {
            var inst_id = id_vvs_gcs_18_19;
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };

            var dtos = new List<GcsStuDto>();
            var stu_lines = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "gcs_ct_eems_stu.csv"));
            foreach (var line in stu_lines)
            {
                var arr = line.Split(",".ToCharArray());
                dtos.Add(new GcsStuDto()
                {
                    Id = arr[0],
                    Name = arr[1],
                    StudentId = arr[2]
                });
            }

            using (var session = Store.OpenSession())
            {
                var stu_ids = dtos.Select(o => o.Id);
                var stus = session.Load<Student>(stu_ids);
                foreach (var de in stus)
                {
                    var stu = de.Value;
                    var dto = dtos.FirstOrDefault(o => o.Id == stu.Id);
                    if (dto == null) continue;
                    stu.SetStudentId(inst_id, dto.StudentId);
                    Console.WriteLine($"{stu.Id},{stu.Name},{dto.StudentId}");
                    session.Store(stu);
                }
                session.SaveChanges();
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_gcs_student_dup()
        {
            var inst_id = id_vvs_gcs_18_19;
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            var sb = new StringBuilder();
            var stus = new List<Student>();

            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var qry = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                        .Where(o => o.InstId == inst_id)
                        .OfType<Student>();
                using (var enumerator = session.Advanced.Stream(qry))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var enrls = stu.Enrollments.Count(o => o.InstId == inst_id);
                        if (enrls > 1)
                        {
                            stus.Add(stu);
                        }
                    }
                }
            }
            Console.WriteLine(stus.Count);
            Console.WriteLine(JsonConvert.SerializeObject(stus));
        }

        [TestMethod]
        public void guid_test()
        {
            Guid.TryParse(id_btl_col_18_19, out Guid student_id_guid);
            Guid.TryParse("123", out Guid student_id_guid_2);
            Console.WriteLine(student_id_guid == Guid.Empty);
            Console.WriteLine(student_id_guid_2 == Guid.Empty);
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_student_in_course()
        {
            var inst_id = id_btl_col_20_21;
            var short_name = "BTLCOL";
            //ToDo
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(short_name);
                var qry = new StudentQry(uc, null, policyEngineProvider: pep);
                var stus = qry.GetAll();
                // Console.WriteLine(stus.Count);
                // Console.WriteLine(JsonConvert.SerializeObject(stus));
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "stus.json"), JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_reset_cahce()
        {
            var inst_id = id_vvs_gcs_26_27;
            var uc = new UserContext(null) { InstId = inst_id };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var qry = new StudentQry(uc, rmq, null);
                qry.SetStudentCache(reset: true);
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_student_in_cache()
        {
            var inst_id = id_vvs_gcs_26_27;
            var stus = RedisCache.GetStudentDetails(inst_id);
            var stu = stus.FirstOrDefault(o => o.StudentId == "265002");
            Console.WriteLine(JsonConvert.SerializeObject(stu));

        }


        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_student_count()
        {
            //var inst_id = id_vvs_gcs_18_19;
            //var inst_id = id_vvs_sphs_18_19;
            //var inst_id = id_vvs_sppu_18_19;
            var inst_id = id_vvs_gcs_20_21;
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            var count = 0;
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var qry = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                        .Where(o => o.InstId == inst_id)
                        .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                        .ProjectInto<Student_Inst_Index.Result>();
                using (var enumerator = session.Advanced.Stream(qry))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        count++;
                    }
                }
            }
            Console.WriteLine(count);
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_qry_rte_students_count()
        {
            var inst_id = id_vvs_gcs_24_25;
            var course_id = "1a6a3029-2425-45ec-9500-f37f54f82d5f";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = "VVSGCS"
            };
            var count = 0;
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var qry = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                        .Where(o => o.InstId == inst_id)
                        .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                        .Where(o => o.CourseId == course_id)
                        .Where(o => o.Tags != null && o.Tags.Contains("RTE"))
                        .ProjectInto<Student_Inst_Index.Result>();
                using (var enumerator = session.Advanced.Stream(qry))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        count++;
                    }
                }
            }
            Console.WriteLine(count);
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_students()
        {
            var inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var qry = new StudentQry(uc, null);
                var stus = qry.Get<Student_Inst_Index.Result>(activeStudentsOnly: false, todaysAbsenteesOnly: false);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        public void can_get_all_students()
        {
            var inst_id = id_vvs_gcs_24_25;
            var short_name = "VVSGCS";
            var course_id = "1a6a3029-2425-45ec-9500-f37f54f82d5f";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(short_name);
                // var qry = new StudentQry(uc, null, policyEngineProvider: pep);
                var qry = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                        .Where(o => o.InstId == inst_id)
                        .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                        .Where(o => o.CourseId == course_id)
                       .OfType<Student>();

                var sb = new StringBuilder();

                using (var enumerator = session.Advanced.Stream(qry))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var enrl = stu.Enrollments.First(o => o.InstId == inst_id);
                        var line = $"{enrl.StudentId},{stu.Name},{inst_id}";
                        sb.Append(line);
                        sb.Append(Environment.NewLine);
                    }
                }

                File.WriteAllText(Path.Combine(Path.GetTempPath(), "gcs-7.csv"), sb.ToString());
                // File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_students_with_attendance()
        {
            var inst_id = id_vvs_pu_18_19;
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = "VVSPUC"
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var qry = new StudentQry(uc, null, policyEngineProvider: pep);
                var stus = qry.Get<Student_Inst_Index.Result>(todaysAbsenteesOnly: true, per: 1);
                Console.WriteLine(JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_student_contact()
        {
            var inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var cmd = new StudentCmd(uc, null, policyEngineProvider: pep);
                var qry = new StudentQry(uc, null, policyEngineProvider: pep);
                var limitCoursesTo = new List<string>()
                {
                    // "6b673ade-51a3-4099-bd60-5f34b2ef52ce",
                    // "d91ae84b-853b-47fd-80c4-675d206eca75"
                };
                var limitStudentsTo = new List<string>()
                {
                    // "2556fba7-643f-400f-8b2a-be318338a377"
                };
                var stus = cmd.GetContacts(limitCoursesTo, limitStudentsTo);
                Console.WriteLine(JsonConvert.SerializeObject(stus));
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "StudentContact.txt"), JsonConvert.SerializeObject(stus));
            }
        }
        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_student_contact_from_facet()
        {
            var inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var cmd = new StudentCmd(uc, null, policyEngineProvider: pep);
                var recps = get_email_recipient_post_data();
                var stus = cmd.GetContacts(recps);
                Console.WriteLine(JsonConvert.SerializeObject(stus));
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "StudentContact.txt"), JsonConvert.SerializeObject(stus));
            }
        }

        public class GcsStuDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string StudentId { get; set; }
        }

        private static RecipientPostData get_recipient_post_data()
        {
            string post_data = @"
                {  
                  'Recipients':{  
                     'Parent':[
                        [
                           '*',
                           '*',
                           '*',
                           '*'
                        ]
    
                     ],
                     'Student':[]
                    },
                  'Text':'SMS test msg'
                }";
            var receipient_post_data = JsonConvert.DeserializeObject<RecipientPostData>(post_data);
            return receipient_post_data;
        }

        private static RecipientsFacet get_email_recipient_post_data()
        {
            string post_data = @"
                {  
                    'Parent':[
                       ['e2e70d87-af8d-4aa4-ac89-4e4d9ca83332','Annual','*','2378873d-5dcf-40cf-a08c-a1b9f32d25c3']
                    ],
                    'Student':[
                        
                    ]
                }";
            var email_receipient_post_data = JsonConvert.DeserializeObject<RecipientsFacet>(post_data);
            return email_receipient_post_data;
        }

        [TestMethod]
        public void can_get_students_for_exam_facet()
        {
            var inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";
            var courseId = "83257ecb-b4b5-46bc-8964-16ea8fdd046f";
            var section = "";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var qry = new StudentQry(uc, null, policyEngineProvider: pep);
                var stus = qry.GetStudentsForExamFacets(courseId);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        public void can_get_recipient_dto_from_stu_ids()
        {
            var inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(short_name);
                var cmd = new StudentCmd(uc, null, policyEngineProvider: pep);
                var stus = cmd.GetSMSRecipientDtoFromIds(new List<string>() { "7d94aa5b-8d52-4b54-b8e3-ccf8803a761e" });
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        public void can_create_student_job()
        {
            var inst_id = id_btl_col_18_19;
            var student_id = Guid.NewGuid().ToString();
            var short_name = "BTLCOL";
            DateTime doa = DateTime.Parse("2018/10/31");
            DateTime dob = DateTime.Parse("2002/10/31");
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var cmd = new StudentCmd(uc, null, policyEngineProvider: pep);
                //var stu = cmd.Create("5894 5689 2103", "1", "10", "PUC I", "", doa, dob, "abc@test.com", GenderEnum.Female, "", "Archana", "A", "BTL15", "Annual", "PCMB", id_btl_col_18_19, "");
                // var job = new Job() { title = "student_created", payload = new { inst_id = inst_id, student_id = stu.Id } };
                // rmq.Push(job, job.title.Replace("_", "."));
            }

        }

        [TestMethod]
        public void can_update_student_job()
        {
            var inst_id = id_btl_col_18_19;
            var student_id = "ea682c3a-7471-46a0-a527-65ae45a44957";
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var job = new Job() { title = "student_updated", payload = new { inst_id = inst_id, student_id = student_id } };
                rmq.EnqueStudentUpdatedJob(job);
            }
        }

        [TestMethod]
        public void can_disable_student_job()
        {
            var inst_id = id_btl_col_18_19;
            var student_id = "ea682c3a-7471-46a0-a527-65ae45a44957";
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var job = new Job() { title = "student_disabled", payload = new { inst_id = inst_id, student_id = student_id } };
                rmq.EnqueStudentDisabledJob(job);
            }
        }

        [TestMethod]
        public void can_enable_student_job()
        {
            var inst_id = id_btl_col_18_19;
            var student_id = "ea682c3a-7471-46a0-a527-65ae45a44957";
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var job = new Job() { title = "student_enabled", payload = new { inst_id = inst_id, student_id = student_id } };
                rmq.EnqueStudentEnabledJob(job);
            }
        }

        [TestMethod]
        public void can_send_absentee_sms()
        {
            var inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";
            var periodNo = 1;
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var cmd = new StudentCmd(uc, rmq, policyEngineProvider: pep);
                var sms = cmd.SmsAbsentees(periodNumber: periodNo);
            }
        }

        [TestMethod]
        public void can_get_students_flat_data()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";
            var course_id = "1a6a5024-2021-45ec-9500-f37f54f82d5f";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var qry = new StudentQry(uc, null, policyEngineProvider: pep);
                var stus = qry.GetFlatStudentData();
                // stus = stus.Where(o => o.CourseId == course_id && o.Status == "Active").ToList();
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "2.json"), JsonConvert.SerializeObject(stus));
            }
        }

        [TestMethod]
        public void can_promote_student()
        {
            var inst_id = id_btl_col_18_19;
            var short_name = "BTLCOL";

            string studentId = "2c6bb600-4aee-4abe-a61f-2ed23fc8e2fb";
            string baseInstId = id_btl_col_18_19;
            string baseCourseId = "d8bd4288-9d10-4116-9500-2a85bcd7e801";
            string baseTerm = "Annual";
            string baseSection = "B";
            string targetInstId = "77f710c6-927c-4a6f-8c63-79951e4cd1b4";
            string targetCourseId = "7bbfef0a-7d40-49ed-a486-a7735e4949ca";
            string targetTerm = "Annual";
            string targetSection = "B";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var cmd = new StudentCmd(uc, rmq, policyEngineProvider: pep);
                cmd.Promote(studentId, baseInstId, baseCourseId, baseTerm, baseSection, targetInstId, targetCourseId, targetTerm, targetSection);
            }
        }

        [TestMethod]
        public void can_fix_student_name()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     //  .Where(o => o.CourseId == "1a6a3020-1920-45ec-9500-f37f54f82d5f")
                                     .OfType<Student>();
                var sb = new StringBuilder();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var enrl = stu.Enrollments.First(o => o.InstId == inst_id);
                        if (enrl != null)
                        {
                            if (enrl.CourseId == "1a6a3020-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "Playhome";
                            else if (enrl.CourseId == "1a6a3021-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "KG I";
                            else if (enrl.CourseId == "1a6a3022-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "KG II";
                            else if (enrl.CourseId == "1a6a3023-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "I";
                            else if (enrl.CourseId == "1a6a3024-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "II";
                            else if (enrl.CourseId == "1a6a3025-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "III";
                            else if (enrl.CourseId == "1a6a3026-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "IV";
                            else if (enrl.CourseId == "1a6a3027-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "V";
                            else if (enrl.CourseId == "1a6a3028-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "VI";
                            else if (enrl.CourseId == "1a6a3029-2021-45ec-9500-f37f54f82d5f") enrl.CourseName = "VII";
                            session.Store(stu);
                        }
                        // if (!stu.FirstName.Contains(".")) continue;
                        // var fn = stu.FirstName;
                        // var mn = stu.MiddleName;
                        // var ln = stu.LastName;
                        // var name = stu.Name;
                        // stu.Name = name;
                        // sb.AppendLine($"<p>{stu.Id} : {fn} {mn} {ln} => {stu.Name}</p>");
                    }
                    session.SaveChanges();
                }
                // File.WriteAllText(Path.Combine(Path.GetTempPath(), "2.html"), sb.ToString());
            }
        }

        [TestMethod]
        public void can_fix_spaces_in_student_ids()
        {
            var inst_id = "39dc45ca-662d-4e63-8b51-e97639d7db57";
            var short_name = "AKSHFN";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .OfType<Student>()
                                     .ToList();
                var sb = new StringBuilder();
                foreach (var stu in students)
                {
                    var enrl = stu.Enrollments.First(o => o.InstId == inst_id);
                    var id = enrl.StudentId;
                    if (id != null)
                    {
                        var id_new = Regex.Replace(id, @"\s", "");
                        stu.StudentId = id_new;
                        enrl.StudentId = id_new;
                        sb.AppendLine($"{stu.Id} : {id} => {id_new}");
                        session.Store(stu);
                    }
                }
                session.SaveChanges();

                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), sb.ToString());
            }
        }

        [TestMethod]
        public void can_get_disabled_students()
        {
            var inst_id = "39dc45ca-662d-4e63-8b51-e97639d7db57";
            var short_name = "AKSHFN";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);

                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     //.Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Disabled)
                                     .OfType<Student>()
                                     .ToList();
                // var sb = new StringBuilder();
                // foreach (var stu in students)
                // {
                //     sb.AppendLine($"{stu.Id}");
                //     sb.AppendLine(Environment.NewLine);
                // }

                Console.WriteLine(students.Count);

                // File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), sb.ToString());
            }
        }
        [TestMethod]
        public void can_fix_repeating_student_names()
        {
            var inst_id = "39dc45ca-662d-4e63-8b51-e97639d7db57";
            var short_name = "AKSHFN";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     //  .Where(o => o.CourseId == "1a6a3020-1920-45ec-9500-f37f54f82d5f")
                                     .OfType<Student>();
                var sb = new StringBuilder();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var name = stu.Name;
                        var name_sanitized = Helper.DeduplicatePattern(name);
                        if (name != name_sanitized)
                            sb.AppendLine($"<p>{stu.Id} : {name} => {name_sanitized}</p>");
                    }

                }
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), sb.ToString());
            }
        }

        [TestMethod]
        public void can_fix_admission_numbers()
        {
            var inst_id = "39dc45ca-662d-4e63-8b51-e97639d7db57";
            var short_name = "AKSHFN";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     //.Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Disabled)
                                     .OfType<Student>();
                var sb = new StringBuilder();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        string admission_no = stu.Enrollments.FirstOrDefault().AdmissionNo;
                        if (admission_no.Contains("K"))
                        {
                            var new_no = admission_no.Replace('K', 'F');
                            sb.AppendLine($"<p>{stu.Id}, {stu.Name} : {admission_no} => {new_no}</p>");
                            stu.Enrollments.FirstOrDefault().AdmissionNo = new_no;
                            stu.Enrollments.FirstOrDefault().StudentId = new_no;
                            session.Store(stu);
                        }

                    }
                    session.SaveChanges();
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "2.html"), sb.ToString());
                }
            }
        }

        [TestMethod]
        public void can_get_enrl_studentId()
        {
            var studentId = "01702e5d-3930-4d19-bbdb-6feaf020a86d";
            Guid.TryParse(studentId, out Guid guid);
            var x = guid == Guid.Empty ? studentId : "";
            Console.WriteLine(guid.ToString());
            Console.WriteLine(guid == Guid.Empty);
            Console.WriteLine(x);
        }

        [TestMethod]
        public void can_promote_sppu_students()
        {
            var inst_id = id_vvs_gcs_23_24;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var file = File.ReadAllText(Path.Combine(Path.GetTempPath(), "prod_04_vvsphs_2425_coursemaps.json"));
                List<CourseMap> course_maps = JsonConvert.DeserializeObject<List<CourseMap>>(file);
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var cmd = new StudentCmd(uc, rmq, policyEngineProvider: pep);
                var qry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                var students = qry.GetAll();

                var stu_promtion_list = new List<StuPromotionDto>();
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
                            stu_pro_dto.TargetSection = stu.SectionName;
                            stu_promtion_list.Add(stu_pro_dto);
                        }
                    }
                }

                File.WriteAllText(Path.Combine(Path.GetTempPath(), "sphs_stu_promotion.txt"), JsonConvert.SerializeObject(stu_promtion_list));
            }
        }

        public class CourseMap
        {
            public string BaseCourseId { get; set; }
            public string TargetCourseId { get; set; }
            public string BaseInstId { get; set; }
            public string TargetInstId { get; set; }
        }

        [TestMethod]
        public void can_get_students_contacts()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var json = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "I.csv"));
                var json1 = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "gcs-1.json"));

                var sb = new StringBuilder();
                string file_path = Path.Combine(Path.GetTempPath(), "contact.csv");
                using (StreamWriter output_file = new StreamWriter(file_path))
                {
                    foreach (var line in json)
                    {
                        var stu = JsonConvert.DeserializeObject<StuContact>(line);
                        var student = session.Load<Student>(stu.Id);
                        stu.Mobile = stu.Mobile;

                        sb.Append(stu);
                        sb.Append(Environment.NewLine);
                    }
                    output_file.WriteLine(sb.ToString());
                }
            }
        }

        [TestMethod]
        public void can_get_students_contacts_for_result()
        {
            var inst_id = id_vvs_pu_21_22;
            var short_name = "VVSPUC";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                                     .Where(o => o.InstId == inst_id)
                                                     .Where(o => o.CourseId == "1a6a6027-2122-45ec-9500-f37f54f82d5f" || o.CourseId == "1a6a6023-2122-45ec-9500-f37f54f82d5f")
                                                     .OfType<Student>()
                                                     .OrderBy(o => o.Name)
                                                     .ToList();

                var results = new List<StuContactForResult>();
                foreach (var line in students)
                {
                    var dto = new StuContactForResult();
                    dto.Mobile = line.Mobile;
                    dto.Name = line.Name;
                    results.Add(dto);
                }
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "stus-science.csv"), JsonConvert.SerializeObject(results));
            }
        }

        public class StuContactForResult
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Mobile { get; set; }
        }

        public class StuContact
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string PersonaId { get; set; }
            public string Mobile { get; set; }
        }



        [TestMethod]
        public void can_change_student_section()
        {
            var inst_id = id_vvs_gcs_26_27; //id_vvs_sppu_19_20;
            var short_name = "VVSGCS";
            //var course = "PUC I";
            var course_id = "1a6a3021-2627-45ec-9500-f37f54f82d5f";
            // var branch = "EABC";
            var old_section = "D";
            var new_section = "B";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                        .Where(o => o.InstId == inst_id)
                        .Where(o => o.CourseId == course_id && o.SectionName == old_section)
                        .OfType<Student>()
                        .ToList();

                foreach (var stu in students)
                {
                    stu.UpdateSectionInEnrollment(inst_id, new_section);
                    session.Store(stu);
                }
                session.SaveChanges();
            }
        }

        [TestMethod]
        public void can_get_students_firstname()
        {
            var inst_id = id_vvs_gcs_19_20;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                      .OfType<Student>();
                var sb = new StringBuilder();
                sb.AppendLine($"first_name,gender");
                var list = new List<string>();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var first_name = stu.FirstName;
                        var gender = stu.Gender;
                        var name_sanitized = Helper.Sanitize(first_name);
                        name_sanitized = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name_sanitized.ToLower());
                        string line = $"{name_sanitized},{gender}";
                        if (!list.Contains(line))
                        {
                            list.Add(line);
                            sb.AppendLine(line);
                        }
                    }
                }
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "firstname.csv"), sb.ToString());
            }
        }

        [TestMethod]
        public void can_get_students_id()
        {
            var inst_id = id_vvs_gcs_24_25;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                      .OfType<Student>();
                var sb = new StringBuilder();
                sb.AppendLine($"student_id,owner_id");
                var list = new List<string>();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var student_id = stu.StudentId;
                        string line = $"{student_id},{inst_id}";

                        list.Add(line);
                        sb.AppendLine(line);
                    }
                }
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "gcs.csv"), sb.ToString());
            }
        }

        [TestMethod]
        public void can_get_students_lastname()
        {
            var inst_id = id_vvs_gcs_19_20;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                      .OfType<Student>();
                var sb = new StringBuilder();
                sb.AppendLine($"last_name");
                var list = new List<string>();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var last_name = stu.LastName;
                        var name_sanitized = Helper.Sanitize(last_name);
                        name_sanitized = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name_sanitized.ToLower());
                        string line = $"{name_sanitized}";
                        if (!list.Contains(line))
                        {
                            list.Add(line);
                            sb.AppendLine(line);
                        }
                    }
                }
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "lastname.csv"), sb.ToString());
            }
        }
        [TestMethod]
        [Ignore]
        public void can_set_institute_email_for_student()
        {
            var inst_id = id_vvs_sppu_21_22;
            var short_name = "VVSSPU";
            var course_id = "1a6a5026-2122-45ec-9500-f37f54f82d5f";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .Where(o => o.CourseId == course_id)
                            .OfType<Student>();
                using (var enumerator = session.Advanced.Stream(stus))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        stu.SetInstituteEmail(stu.VirtualId.Replace("@campustrack.net", "@vvssppuc.org"));
                        session.Store(stu);
                    }
                    session.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void can_get_student_contacts()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.Status == StudentStatusEnum.Active)
                                     .Where(o => o.CourseId == "1a6a3020-2021-45ec-9500-f37f54f82d5f")
                                     .OfType<Student>();
                var res = new List<UserData>();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var mobiles = new List<string>();
                        var fm = stu.Father.GetPrimaryMobile();
                        var mm = stu.Mother.GetPrimaryMobile();
                        if (!string.IsNullOrEmpty(fm)) mobiles.Add(fm);
                        if (!string.IsNullOrEmpty(mm)) mobiles.Add(mm);
                        var dto = new UserData();
                        dto.Name = stu.Name;
                        dto.Email = stu.VirtualId;
                        dto.Password = stu.FirstName + stu.Id.Substring(stu.Id.Length - 4);
                        dto.Mobile = string.Join(",", mobiles.ToArray());
                        res.Add(dto);
                    }
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "new-gcs-kg2.json"), JsonConvert.SerializeObject(res));
                }
            }
        }

        [TestMethod]
        public void can_set_course()
        {
            var inst_id = id_vvs_sphs_23_24;
            var short_name = "VVSPHS";
            var course_id = "1a6a4022-2424-45ec-9500-f37f54f82d5f";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .Where(o => o.CourseId == course_id)
                            .OfType<Student>();
                using (var enumerator = session.Advanced.Stream(stus))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var enrl = stu.Enrollments.FirstOrDefault(o => o.CourseId == course_id);
                        enrl.CourseId = "1a6a4022-2425-45ec-9500-f37f54f82d5f";
                        enrl.CourseName = "Xth";
                        session.Store(stu);
                    }
                    session.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void can_get_students_based_on_tags()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";
            var tag = "ios";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.Status == StudentStatusEnum.Active)
                                     .Where(o => o.Tags.Contains(tag))
                                     .OfType<Student>();
                var result = new List<StudentInfo>();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var studentInfo = new StudentInfo();
                        var enrl = stu.Enrollments.First(o => o.InstId == inst_id);
                        studentInfo.Id = stu.Id;
                        studentInfo.Name = stu.Name;
                        studentInfo.Course = enrl.CourseName;
                        studentInfo.Section = enrl.SectionName;
                        result.Add(studentInfo);
                    }
                    result = result.OrderByDescending(o => Enumerable.Reverse(pe.CourseOrder()).ToList().IndexOf(o.Course)).ThenBy(o => o.Section).ToList();
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "students.json"), JsonConvert.SerializeObject(result));
                    Console.WriteLine(result.Count);
                }
            }
        }

        public class UserData
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Mobile { get; set; }
        }

        [TestMethod]
        public void get_stu_id_and_course()
        {
            var inst_id = id_vvs_gcs_20_21;
            var inst_short_name = "VVSGCS";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = inst_short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);

                var courses = RedisCache.GetCourseDto(inst_id);

                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                                     .OfType<Student>();
                var sb = new StringBuilder();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var enrl = stu.Enrollments.Find(o => o.InstId == inst_id);
                        if (enrl != null)
                        {
                            var stu_course = courses.Find(o => o.Id == enrl.CourseId);
                            var crs = stu_course.Name == "Playhome" ? "ph" : stu_course.Name == "KG I" ? "nursery" : stu_course.Name == "KG II" ? "nursery" : "std";
                            sb.AppendLine($"{stu.Id},{crs}");
                        }
                    }
                }
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "student_fee_id.csv"), sb.ToString());
            }
        }

        [TestMethod]
        public void student_not_as_user()
        {
            var inst_id = id_vvs_gcs_21_22;
            var inst_short_name = "VVSGCS";
            var student_persona_id = "1a6a3014-2122-45ec-9500-f37f54f82d5f";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = inst_short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);

                var users = session.Query<User>()
                                   .Where(u => u.Personas.Contains(student_persona_id))
                                   .ToList();

                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                                     .OfType<Student>();
                var sb = new StringBuilder();
                var list = new List<StuUserDto>();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var usr = users.Find(o => o.Email == stu.VirtualId);
                        if (usr == null)
                        {
                            var dto = new StuUserDto();
                            dto.Email = stu.VirtualId;
                            dto.UserId = string.Empty;
                            dto.StudentId = stu.Id;
                            list.Add(dto);
                        }
                    }

                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "students_not_as_users_list.json"), JsonConvert.SerializeObject(list));
                }
            }
        }

        [TestMethod]
        public void set_user_id_for_students()
        {
            var inst_id = id_vvs_sppu_20_21;
            var inst_short_name = "VVSSPU";
            var student_persona_id = "1a6a5014-2021-45ec-9500-f37f54f82d5f";
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = inst_short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);

                var users = session.Query<User>()
                                   .Where(u => u.Personas.Contains(student_persona_id))
                                   .ToList();

                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                                     .OfType<Student>();
                using (var enumerator = session.Advanced.Stream(students))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var email = stu.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email && o.Location == LocationType.Institute).FirstOrDefault();
                        if (email != null)
                        {
                            var usr = users.Find(o => o.Email == email.Info);
                            if (usr != null)
                            {
                                stu.UserId = usr.Id;
                                session.Store(stu);
                            }
                        }
                    }
                    session.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void can_get_students_attendance_widget_data()
        {
            var inst_id = id_vvs_pu_21_22;
            var short_name = "VVSPUC";

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var qry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                var students = qry.GetAttendanceWidgetData();
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "att.json"), JsonConvert.SerializeObject(students));
            }
        }

        [TestMethod]
        public void can_get_student_count_institute_genderwise()
        {
            var inst_id = id_vvs_gcs_18_19;
            var short_name = "VVSGCS";

            List<string> institute_ids = new List<string>() { id_vvs_gcs_18_19, id_vvs_gcs_19_20 };

            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var qry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                var students = qry.GetStudentCountInstituteGenderwise(institute_ids);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "stu_count.json"), JsonConvert.SerializeObject(students));
            }
        }

        [TestMethod]
        public void can_get_student_count_institutewise()
        {
            var inst_id = id_vvs_gcs_18_19;
            var short_name = "VVSGCS";

            List<string> institute_ids = new List<string>() { id_vvs_gcs_18_19, id_vvs_gcs_19_20 };

            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var qry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                var students = qry.GetStudentCountInstitutewise(institute_ids);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "stu_count.json"), JsonConvert.SerializeObject(students));
            }
        }

        [TestMethod]
        public void can_get_recipients()
        {
            var inst_id = id_btl_col_20_21;
            var short_name = "BTLCOL";
            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var qry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                var recps = get_email_recipient_post_data();
                var students = qry.GetSubscriptionRecipients(recps);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "recipients.json"), JsonConvert.SerializeObject(students));
            }
        }

        [TestMethod]
        public void can_get_all_passes_out_students()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";
            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var pe = pep.For(uc.ShortName);
                var qry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                var students = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                                     .Where(o => o.CourseId == "1a6a3029-2021-45ec-9500-f37f54f82d5f")
                                     .OfType<Student>();
                var list = new List<string>();
                foreach (var stu in students)
                {
                    list.Add(stu.VirtualId);
                }
                // foreach (var stu in students)
                // {
                //     var emails = stu.EmailCSV.Split(",").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                //     var email = emails.Find(o => o.EndsWith(".org"));
                //     list.Add(email);
                // }
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "students.json"), JsonConvert.SerializeObject(list));
            }
        }

        [TestMethod]
        public void assign_sec()
        {
            var inst_id = id_vvs_gcs_23_24;
            var short_name = "VVSGCS";
            var course_id = "1a6a3023-2324-45ec-9500-f37f54f82d5f";
            var section = "D";
            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .Where(o => o.CourseId == course_id)
                            .OfType<Student>()
                            .ToList();
                string file = Path.Combine(Path.GetTempPath(), "Id.csv");
                var csv = File.ReadAllLines(file);
                var rows = csv.Skip(1);
                var stus_not_found = new List<string>();
                foreach (var row in rows)
                {
                    var stu = stus.FirstOrDefault(o => o.Name == row);
                    if (stu != null)
                    {
                        var enrl = stu.Enrollments.FirstOrDefault(o => o.CourseId == course_id);
                        enrl.SectionName = section;
                        session.Store(stu);
                    }
                    else
                    {
                        stus_not_found.Add(row);
                    }
                }
                Console.WriteLine(stus_not_found.Count);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "stus-D"), JsonConvert.SerializeObject(stus_not_found));

                session.SaveChanges();
            }
        }

        [TestMethod]
        public void can_change_enrol()
        {
            var inst_id = "761650eb-5b4c-4967-912a-6ca2963dc004";
            var short_name = "BITSCH";

            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            //.Where(o => o.CourseId == course_id)
                            .OfType<Student>();
                using (var enumerator = session.Advanced.Stream(stus))
                {
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var enrl = stu.Enrollments[0];
                        enrl.CourseId = "";
                        enrl.CourseName = "";
                        enrl.SectionName = "";
                        enrl.InstId = "eabdb3ea-d348-478b-975a-5cd53239841e";
                        session.Store(stu);
                    }
                    session.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void can_change_enrol1()
        {
            var inst_id = "96379c5f-2021-43ff-808d-5f2b1bb64144";
            var short_name = "BTLSCH";

            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .OfType<Student>()
                            .Take(28);
                // .ToList();
                //  File.WriteAllText(Path.Combine(Path.GetTempPath(), "1A"), JsonConvert.SerializeObject(stus));            
                using (var enumerator = session.Advanced.Stream(stus))
                {
                    var rmq = new RabbitMQService(Config);
                    var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                    var qry = new StudentQry(uc, null, pep);
                    var cmd = new StudentCmd(uc, rmq, AutofacContainer.Resolve<IPolicyEngineProvider>());
                    while (enumerator.MoveNext())
                    {
                        var stu = enumerator.Current.Document;
                        var courseName = "II";
                        var courseId = "2a6a3024-2324-45ec-9500-f37f54f82d5f";
                        var sec = "B";

                        // cmd.Create(null, null, null, courseName, courseId, DateTime.MinValue, null, "", stu.Gender, "", stu.Name, sec, "", "Annual", null, "eabdb3ea-d348-478b-975a-5cd53239841e", "");
                        // var enrl = stu.Enrollments[0];
                        // enrl.CourseId = "2a6a3024-2324-45ec-9500-f37f54f82d5f";
                        // enrl.CourseName = "II";
                        // enrl.SectionName = "B";
                        // session.Store(stu);
                    }
                    //session.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void regex_match()
        {
            string input = "Glevina Pratiksha D'Sa";
            string result = Regex.Replace(input, @"[^a-zA-Z']", " ");
            Console.WriteLine(result);
        }

        public class TransactionDto
        {
            public string Id { get; set; }
            public string StudentId { get; set; }
            public string TxNo { get; set; }
            public string Status { get; set; }
        }

        [TestMethod]
        public void can_set_fee_receivable()
        {
            var json = File.ReadAllText(Path.Combine(Path.GetTempPath(), "students_fr.json"));
            var transactions = JsonConvert.DeserializeObject<List<TransactionDto>>(json);
            var studentIds = transactions.Select(t => t.StudentId).ToList();

            var inst_id = id_vvs_gcs_26_27;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;

                var students = session
                    .Query<Student_Inst_Index.Result, Student_Inst_Index>()
                    .Where(o => o.InstId == inst_id)
                    // .Where(o => o.CourseId == "1a6a3023-2627-45ec-9500-f37f54f82d5f")
                    .OfType<Student>()
                    .ToList();

                Console.WriteLine($"Total students in DB: {students.Count}");

                students = students.Where(s => studentIds.Contains(s.Id)).ToList();
                var count = students.Count;

                File.WriteAllText(Path.Combine(Path.GetTempPath(), "students_ids.json"), JsonConvert.SerializeObject(studentIds));

                Console.WriteLine($"Total students matching transactions: {count}");

                var studentIdsInDb = students.Select(s => s.Id).ToList();

                var missingFromDb = studentIds
                    .Where(sid => !studentIdsInDb.Any(dbId => dbId.Contains(sid)))
                    .ToList();

                Console.WriteLine($"Missing from DB: {missingFromDb.Count}");
                foreach (var id in missingFromDb)
                    Console.WriteLine(id);

                foreach (var stu in students)
                {
                    var tx = transactions.FirstOrDefault(t => t.StudentId == stu.Id);

                    var dto = new FeesReceivable()
                    {
                        InstId = inst_id,
                        FeeId = "267a0a26-2a04-456b-94ea-6cd90fb30a21",
                        FeeName = "CEP 2026-27",
                        TxId = tx.Id,
                        Amount = 10000,
                        TxStatus = tx.Status == "1" ? TxStatusEnum.Active : TxStatusEnum.Disabled
                    };
                    stu.FeesReceivable.Add(dto);
                    // session.Store(stu);
                }
                // session.SaveChanges();

            }

        }

        [TestMethod]
        public void can_get_students_fees_receivable()
        {
            var inst_id = id_vvs_gcs_26_27;
            var short_name = "VVSGCS";

            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;

                var students = session
                        .Query<Student_Inst_Index.Result, Student_Inst_Index>()
                        .Where(o => o.InstId == inst_id)
                        .ToList();

                students = students
                    .Where(s => s.FeesReceivable != null && s.FeesReceivable.Any(f => f.FeeId == "267a0a26-2a04-456b-94ea-6cd90fb30a21" && f.TxId != string.Empty))
                    .ToList();

                Console.WriteLine($"Total students with fees receivable: {students.Count}");
                //  File.WriteAllText(Path.Combine(Path.GetTempPath(), "students_fees_receivable.json"), JsonConvert.SerializeObject(students));

                foreach (var stu in students)
                {
                    var fr = stu.FeesReceivable.First(f => f.FeeId == "267a0a26-2a04-456b-94ea-6cd90fb30a21" && f.TxId != string.Empty);
                    Console.WriteLine($"StudentId: {stu.Id}, TxId: {fr.TxId}, TxStatus: {fr.TxStatus}, Amount: {fr.Amount}");
                }
            }
        }

        [TestMethod]
        public void can_get_student_ids_in_course_and_excluded_student_ids()
        {
            var inst_id = id_vvs_bms_26_27;
            var short_name = "VVSBMS";

            var courseIds = new[]
            {
                "1a6a7021-2627-45ec-9500-f37f54f82d5f",
                "1a6a7022-2627-45ec-9500-f37f54f82d5f"

            };

            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;

                var excludedNames = new HashSet<string>(new[] {
                  "CHETHAN KUMAR S D",
"KANVI R SHAM",
"NISHIKA A GOWDA",
"SHANAYA RAJ",
"SRI LASKHMI S",
"SRIVAARI Y",
"TANUSH R",
"VIHANA J",
"VISHAK S",
"BHAVANYA RAJ B",
"CHARVI RAO S",
"CHERIKA D",
"LIHAN M GOWDA",
"PARISHKA M",
"SANVITH M SURYA",
"SHREEYA V GOWDA",
"SURAKSHA DEVAMSH",
"YADHAVI KUMAR",
"NAMASVI N SHETTY",
"RIHAN PRASAD M G",
"VISHRUTH  V",
"NAMASVI GURU",
"RIAAN S ARADHYA",
"THANUSH H"
                }, StringComparer.OrdinalIgnoreCase);

                var students = session
                    .Query<Student_Inst_Index.Result, Student_Inst_Index>()
                    .Where(o => o.InstId == inst_id)
                    .Where(o => o.CourseId.In(courseIds))
                    .OfType<Student>()
                    .ToList();

                // Students whose names ARE in the exclusion list
                List<string> excludedStudentIds = students
                    .Where(s => excludedNames.Contains(s.Name))
                    .Select(s => s.Id)
                    .ToList();

                // Students whose names are NOT in the exclusion list
                List<string> remainingStudentIds = students
                    .Where(s => !excludedNames.Contains(s.Name))
                    .Select(s => s.Id)
                    .ToList();

                File.WriteAllText(Path.Combine(Path.GetTempPath(), "excluded_student_ids-l&u.json"), JsonConvert.SerializeObject(excludedStudentIds));
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "remaining_student_ids-l&u.json"), JsonConvert.SerializeObject(remainingStudentIds));

                Console.WriteLine($"Excluded  : {excludedStudentIds.Count}");
                Console.WriteLine($"Remaining : {remainingStudentIds.Count}");
                Console.WriteLine($"Total     : {students.Count}"); // sanity: excluded + remaining == total
            }
        }

        [TestMethod]
        public void can_assign_students_to_section_with_list()
        {
            var inst_id = id_vvs_gcs_26_27;
            var short_name = "VVSGCS";
            var course_id = "1a6a3027-2627-45ec-9500-f37f54f82d5f";
            var section = "A";
            var uc = new UserContext(null)
            {
                OrgId = inst_id,
                ShortName = short_name
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var stus = session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                            .Where(o => o.InstId == inst_id)
                            .Where(o => o.CourseId == course_id)
                            .OfType<Student>()
                            .ToList();

                var rows = new List<string>()
        {
     "Aaddhya P Kushtagi",
    "Advaith Girish Murthy",
    "Adwetiya S",
    "Amartya A Hakkekatte",
    "Anshika Prakash Patil",
    "D Sanmith Aryan",
    "Danish V",
    "Diya Vikram",
    "Gaanavi S",
    "Isha R",
    "Keshava Kavital",
    "Lakshita T",
    "Manogna N",
    "Manvith N",
    "Mokshagna S Gururaja",
    "N V Aavaith",
    "Nandan C Gowda",
    "Nihanth V",
    "Nikitha Rao S U",
    "Nithin D",
    "Parishkriti S Kulkarni",
    "Raghurama",
    "Ranganatha R",
    "Rishika L",
    "Sai Sanghvi",
    "Sajethsamart Gowda",
    "Samitha S",
    "Sanvi R Koushik",
    "Shlok Namratha Phalgun",
    "Sriaarna V D",
    "Srujan N",
    "Subisha L",
    "Unnati M",
    "Vaibhav P S",
    "Varun A S"
        };
                var stus_not_found = new List<string>();
                foreach (var row in rows)
                {
                    var stu = stus.FirstOrDefault(o => o.Name == row);
                    if (stu != null)
                    {
                        var enrl = stu.Enrollments.FirstOrDefault(o => o.CourseId == course_id);
                        enrl.SectionName = section;
                 //       session.Store(stu);
                    }
                    else
                    {
                        stus_not_found.Add(row);
                    }
                }
                Console.WriteLine(stus_not_found.Count);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "stus-B"), JsonConvert.SerializeObject(stus_not_found));

               // session.SaveChanges();
            }
        }



        public class StuUserDto
        {
            public string Email { get; set; }
            public string UserId { get; set; }
            public string StudentId { get; set; }
        }

        public class User
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public List<string> Personas { get; set; }
        }
    }
}