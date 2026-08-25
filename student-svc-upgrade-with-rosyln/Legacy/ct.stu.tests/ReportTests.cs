using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using ct._Core.Policy;
using ct._Core.Security;
using ct._Core.Service;
using ct.stu.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Repo.Db.Query;

namespace ct.stu.tests
{
    [TestClass]
    public class ReportTests : TestBase
    {
        [ClassInitialize]
        public static void class_init(TestContext context)
        {
            Configure();
            ConfigureStore();
        }

        [TestInitialize]
        public void test_init()
        {
        }
        [TestMethod]
        public void can_get_coursewise_student_breakup()
        {
            var inst_id = id_vvs_gcs_19_20;
            // var course_id = "6b673ade-51a3-4099-bd60-5f34b2ef52ce";
            //var course_id = "all";

            var uc = new UserContext(null) { InstId = inst_id, ShortName = "VVSGCS" };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);
                var model = report_service.GetCoursewiseBreakUpModel();
                var model_json = JsonConvert.SerializeObject(model);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.json"), model_json);


                // var result = report_service.GetCoursewiseBreakUp();
                // File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), result);
            }
        }

        [TestMethod]
        public void can_get_sectionwise_student_breakup()
        {
            var inst_id = id_vvs_sppu_18_19;
            // var course_id = "6b673ade-51a3-4099-bd60-5f34b2ef52ce";
            //var course_id = "all";

            var uc = new UserContext(null) { InstId = inst_id, ShortName = "VVSSPU" };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);
                var result = report_service.GetSectionwiseBreakUp();
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), result);
            }
        }

        [TestMethod]
        public void can_get_student_details_report()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";
            var course_ids = new List<string>() { "1a6a3027-2021-45ec-9500-f37f54f82d5f", "1a6a3028-2021-45ec-9500-f37f54f82d5f" };

            var uc = new UserContext(null) { InstId = inst_id, ShortName = short_name };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);
                var result = report_service.GetStudentDetails(courseIds: course_ids);
                var model = report_service.GetStudentDetailsModel(course_ids);

                var model_json = JsonConvert.SerializeObject(model);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), model_json);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "2.html"), result);
            }
        }

        [TestMethod]
        public void can_get_student_parent_details_report()
        {
            var inst_id = id_vvs_gcs_20_21;
            var short_name = "VVSGCS";
            var course_ids = new List<string>() { "1a6a3028-2021-45ec-9500-f37f54f82d5f" };

            var uc = new UserContext(null) { InstId = inst_id, ShortName = short_name };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);
                var result = report_service.GetStudentParentDetails(null, "1a6a3029-2021-45ec-9500-f37f54f82d5f", "B");
                var model = report_service.GetStudentParentDetailsModel("1a6a3029-2021-45ec-9500-f37f54f82d5f", "B");

                var model_json = JsonConvert.SerializeObject(model);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), model_json);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "2.html"), result);
            }
        }

        [TestMethod]
        public void can_get_student_parent_details_report_as_csv()
        {
            var inst_id = id_vvs_gcs_24_25;
            var short_name = "VVSGCS";
            var course_id = "1a6a3028-2425-45ec-9500-f37f54f82d5f";
            var section = "A";
            var uc = new UserContext(null) { InstId = inst_id, ShortName = short_name };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);

                var model = report_service.GetStudentParentDetailsAsCSV(course_id, section);
                var model_json = JsonConvert.SerializeObject(model);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), model_json);
            }
        }

        [TestMethod]
        public void can_get_absentees_report()
        {
            var inst_id = id_btl_col_18_19;
            DateTime date = DateTime.Parse("2018/06/08");

            var uc = new UserContext(null) { InstId = inst_id };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var stud_qry = new StudentQry(uc, null);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, null);
                var model = report_service.GetAbsenteesReportModel(date);
                var model_json = JsonConvert.SerializeObject(model);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), model_json);

                var result = report_service.GetAbsenteesReport(date);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "2.html"), result);
            }
        }

        [TestMethod]
        public void can_get_basic_sectionwise_student_breakup()
        {
            var inst_id = id_vvs_gcs_19_20;
            // var course_id = "6b673ade-51a3-4099-bd60-5f34b2ef52ce";
            //var course_id = "all";

            var uc = new UserContext(null) { InstId = inst_id, ShortName = "VVSGCS" };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);
                var result = report_service.GetBasicSectionwiseBreakUp();
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), result);
            }
        }

        [TestMethod]
        public void can_get_new_admissions_report()
        {
            var inst_id = id_vvs_gcs_22_23;
            var uc = new UserContext(null) { InstId = inst_id, ShortName = "VVSGCS" };
            var course_ids = new List<string>() { "1a6a3024-2223-45ec-9500-f37f54f82d5f" };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);

                var model = report_service.GetNewAdmissionsModel(null);
                var model_json = JsonConvert.SerializeObject(model);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), model_json);


                var result = report_service.GetNewAdmissions(course_ids);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "2223.html"), result);
            }
        }

        [TestMethod]
        public void can_get_tc_students_report()
        {
            var inst_id = id_vvs_gcs_21_22;
            var uc = new UserContext(null) { InstId = inst_id, ShortName = "VVSGCS" };
            var course_ids = new List<string>() { };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);

                var model = report_service.GetTCStudentsModel(course_ids);
                var model_json = JsonConvert.SerializeObject(model);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.html"), model_json);

                var result = report_service.GetTCStudentsList(course_ids, null);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "2223.html"), result);
            }
        }

        [TestMethod]
        public void can_get_iep()
        {
            var inst_id = "39dc45ca-662d-4e63-8b51-e97639d7db57";
            var uc = new UserContext(null) { InstId = inst_id, ShortName = "AKSHFN" };
            var student_id = "71503630-9311-4b0e-a27b-09ac815d4828";
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var ghs = new GithubService(Config, uc);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var stud_qry = new StudentQry(uc, null, pep);
                var templ_service = new TemplateService(ghs);
                var report_service = new ReportService(uc, stud_qry, ghs, templ_service, pep);

                var model = report_service.GetIEPModel(student_id);
                var model_json = JsonConvert.SerializeObject(model);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "1.json"), model_json);

                var result = report_service.GetIEP(student_id, null);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "2223.html"), result);
            }
        }
    }
}