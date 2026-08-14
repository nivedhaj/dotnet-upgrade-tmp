using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using ct._Core.Policy;
using ct._Core.Repo.Dto;
using ct._Core.Repo.Resource;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Marten;
using Marten.Linq;
using Marten; using Marten.Linq;
using Repo.Db.Index;
using Repo.Db.Query;
using Repo.Dto;
using Repo.Resource;

namespace ct.stu.Service
{
    /// <summary>
    /// Service for report generation
    /// </summary>
    public class ReportService
    {
        private readonly IUserContext _uc;
        private readonly IDocumentSession _session;
        private readonly StudentQry _studentQry;
        private readonly TemplateService template_service;
        private readonly GithubService github_service;
        private readonly IPolicyEngine _policyEngine;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="studentQry"></param>
        /// <param name="githubService"></param>
        /// <param name="templateService"></param>
        /// <param name="policyEngineProvider"></param>
        public ReportService(
            IUserContext uc,
            StudentQry studentQry,
            GithubService githubService,
            TemplateService templateService,
            IPolicyEngineProvider policyEngineProvider)
        {
            _uc = uc.Current();
            _session = _uc.Session;
            _studentQry = studentQry;
            template_service = templateService;
            github_service = githubService;
            _policyEngine = policyEngineProvider.For(_uc.ShortName);
        }

        /// <summary>
        /// Marks card temp
        /// </summary>
        /// <param name="options"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string GetMarksCard(PrintOptions options = null, bool testData = false)
        {
            //template
            var template = template_service.GetTemplate();
            //model
            var model = GetMarksCardData(testData: testData);
            //report_dto
            //var report_dto = GetAgeAnalysisReportDto(model, options);
            //render
            var result = template_service.Render(template, model);
            return result;
        }

        /// <summary>
        /// Marks card model
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string GetMarksCardData(bool testData = false)
        {
            if (testData)
            {
                var model = template_service.GetModel();
                return model;
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Render Template
        /// </summary>
        /// <param name="options"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string RenderTemp(PrintOptions options = null, bool testData = false)
        {
            //template
            var template = template_service.GetTemplate();
            //model
            var model = RenderTempData(testData: testData);
            //report_dto
            //var report_dto = GetAgeAnalysisReportDto(model, options);
            //render
            var result = template_service.Render(template, model);
            return result;
        }

        /// <summary>
        /// Render Template
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string RenderTempData(bool testData = false)
        {
            if (testData)
            {
                var model = template_service.GetModel();
                return model;
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// View Receipt temp
        /// </summary>
        /// <param name="options"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string ViewFeeReceipt(PrintOptions options = null, bool testData = false)
        {
            //template
            var template = template_service.GetTemplate();
            //model
            var model = ViewFeeReceiptData(testData: testData);
            //report_dto
            //var report_dto = GetAgeAnalysisReportDto(model, options);
            //render
            var result = template_service.Render(template, model);
            return result;
        }

        /// <summary>
        /// View Fee Receipt model
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string ViewFeeReceiptData(bool testData = false)
        {
            if (testData)
            {
                var model = template_service.GetModel();
                return model;
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// View tc template
        /// </summary>
        /// <param name="options"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string ViewTC(PrintOptions options = null, bool testData = false)
        {
            //template
            var template = template_service.GetTemplate();
            //model
            var model = ViewTCData(testData: testData);
            //report_dto
            //var report_dto = GetAgeAnalysisReportDto(model, options);
            //render
            var result = template_service.Render(template, model);
            return result;
        }

        /// <summary>
        /// View tc model
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string ViewTCData(bool testData = false)
        {
            if (testData)
            {
                var model = template_service.GetModel();
                return model;
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Certificate Temp
        /// </summary>
        /// <param name="options"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string Certificate(PrintOptions options = null, bool testData = false)
        {
            //template
            var template = template_service.GetTemplate();
            //model
            var model = CertificateData(testData: testData);
            //report_dto
            //var report_dto = GetAgeAnalysisReportDto(model, options);
            //render
            var result = template_service.Render(template, model);
            return result;
        }

        /// <summary>
        /// Certificate model
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string CertificateData(bool testData = false)
        {
            if (testData)
            {
                var model = template_service.GetModel();
                return model;
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Get Marks For All Students In Exam temp
        /// </summary>
        /// <param name="options"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string GetMarksForAllStudentsInExam(PrintOptions options = null, bool testData = false)
        {
            //template
            var template = template_service.GetTemplate();
            //model
            var model = GetMarksForAllStudentsInExamData(testData: testData);
            //report_dto
            //var report_dto = GetAgeAnalysisReportDto(model, options);
            //render
            var result = template_service.Render(template, model);
            return result;
        }
        /// <summary>
        /// GetMarksForAllStudentsInExam Model
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string GetMarksForAllStudentsInExamData(bool testData = false)
        {
            if (testData)
            {
                var model = template_service.GetModel();
                return model;
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// GetStudentTodayTimetable Template
        /// </summary>
        /// <param name="options"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string GetStudentTodayTimetable(PrintOptions options = null, bool testData = false)
        {
            //template
            var template = template_service.GetTemplate();
            //model
            var model = GetStudentTodayTimetableData(testData: testData);
            //report_dto
            //var report_dto = GetAgeAnalysisReportDto(model, options);
            //render
            var result = template_service.Render(template, model);
            return result;
        }

        /// <summary>
        /// GetStudentTodayTimetable Model
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string GetStudentTodayTimetableData(bool testData = false)
        {
            if (testData)
            {
                var model = template_service.GetModel();
                return model;
            }
            else
                throw new NotImplementedException();
        }

        internal string GetHallTicket(HallTicketDto dto)
        {//context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);
            //model
            var model = GetHallTicketModel();

            //prepare report_data based on model & report options
            var title1 = $"Coursewise Students Breakup";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);

            // print options not sent as part of post data

            var report_data = GetReportData_HallTicket();

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }
        internal List<object> GetHallTicketModel()
        {
            throw new NotImplementedException("Hall ticket model not implemented");
        }
        internal object GetReportData_HallTicket()
        {
            //TBD print options not sent as part of post data

            // options = options == null ? new PrintOptions() { } : options;
            // var report = new ReportDto(options);
            // var chunk_size = report.Options.RecordsPerPage;
            // var iter_indx = 0;
            // var title1 = title.Item1;
            // var title2 = title.Item2;
            // var title3 = title.Item3;
            // var title4 = title.Item4;
            // foreach (var chunk in model.Chunk(chunk_size))
            // {
            //     var page = new Page(options.PrintBorder);
            //     page.SetHeader(inst, iter_indx, report.Options);
            //     page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
            //     page.PC = chunk;
            //     page.SetFooter(inst, iter_indx, report.Options);
            //     report.Pages.Add(page);
            //     iter_indx++;
            // }
            // //serialize & de-serialize to ensure that string-template gets the model correctly.
            // var rpt_str = JsonHelper.Serialize(report);
            // var obj = JsonHelper.Deserialize(rpt_str);
            // return obj;

            return null;
        }

        internal string GetNoDueCertificate(CertificateDto dto)
        {//context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);
            //model
            var model = GetNoDueCertificateModel();

            //prepare report_data based on model & report options
            var title1 = $"Coursewise Students Breakup";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);

            // print options not sent as part of post data

            var report_data = GetReportData_NoDueCertificate();

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }
        internal List<object> GetNoDueCertificateModel()
        {
            throw new NotImplementedException("Hall ticket model not implemented");
        }
        internal object GetReportData_NoDueCertificate()
        {
            //TBD print options not sent as part of post data

            // options = options == null ? new PrintOptions() { } : options;
            // var report = new ReportDto(options);
            // var chunk_size = report.Options.RecordsPerPage;
            // var iter_indx = 0;
            // var title1 = title.Item1;
            // var title2 = title.Item2;
            // var title3 = title.Item3;
            // var title4 = title.Item4;
            // foreach (var chunk in model.Chunk(chunk_size))
            // {
            //     var page = new Page(options.PrintBorder);
            //     page.SetHeader(inst, iter_indx, report.Options);
            //     page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
            //     page.PC = chunk;
            //     page.SetFooter(inst, iter_indx, report.Options);
            //     report.Pages.Add(page);
            //     iter_indx++;
            // }
            // //serialize & de-serialize to ensure that string-template gets the model correctly.
            // var rpt_str = JsonHelper.Serialize(report);
            // var obj = JsonHelper.Deserialize(rpt_str);
            // return obj;

            return null;
        }


        internal string GetStudentDetails(PrintOptions options = null, List<string> courseIds = null)
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            if (options == null) options = new PrintOptions() { RecordsPerPage = 1 };
            if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;

            //model
            var model = GetStudentDetailsModel(courseIds);

            //prepare report_data based on model & report options
            var title1 = $"Student Details";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);
            var report_data = GetReportData_StudentDetails(inst_dto, model, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }

        internal List<StudentDetailsRptDto> GetStudentDetailsModel(List<string> course_ids = null)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<StudentDetailsRptDto>();
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);
            Expression<Func<Student_Inst_Index.Result, bool>> filterByActiveStudents = o => o.EnrollmentStatus == EnrollmentStatusEnum.Active;
            var qry = _session
                            .Query<Student_Inst_Index.Result>()
                            .Where(filterByInst)
                            .Where(filterByActiveStudents)
                            .Where(o => o.CourseId.In(course_ids));

            var stu_list = qry.OfType<Rpt_StudentDto>().ToList();

            var dto = new StudentDetailsRptDto();
            foreach (var stu in stu_list)
            {
                if (stu == null) continue;
                var enrl = stu.Enrollments.Find(o => o.InstId == inst_id);
                if (enrl.Status != EnrollmentStatusEnum.Active) continue;

                var f_email = stu.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault();
                var f_mobile = stu.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();
                var m_email = stu.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault();
                var m_mobile = stu.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();
                var stu_dto = new StudentDetailsDto()
                {
                    Id = stu.Id,
                    StudentId = enrl.StudentId,
                    FirstName = stu.FirstName,
                    LastName = stu.LastName,
                    DOB = stu.DOB.HasValue ? stu.DOB.Value.ToString("dd-MM-yyyy") : string.Empty,
                    CourseName = enrl.CourseName,
                    Section = enrl.SectionName,
                    SENDCategory = 0,
                    Nationality = "Indian",
                    CountryCode = 91,
                    RollNo = enrl.RollNo,
                    StudentGender = stu.Gender,
                    Email = (f_email != null && !string.IsNullOrEmpty(f_email.Info)) ? f_email.Info : (m_email != null && !string.IsNullOrEmpty(m_email.Info)) ? m_email.Info : string.Empty,
                    Mobile = (f_mobile != null && !string.IsNullOrEmpty(f_mobile.Info)) ? f_mobile.Info : (m_mobile != null && !string.IsNullOrEmpty(m_mobile.Info)) ? m_mobile.Info : string.Empty
                };
                dto.StudentList.Add(stu_dto);
                dto.StudentList = dto.StudentList
                                         .OrderByDescending(o => Enumerable.Reverse(_policyEngine.CourseOrder()).ToList().IndexOf(o.CourseName))
                                         .ThenBy(o => o.Section)
                                         .ThenBy(o => o.FirstName)
                                        .ToList();
            }
            result.Add(dto);
            return result;
        }
        internal object GetReportData_StudentDetails(ClientDto inst, List<StudentDetailsRptDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }


        // Student parent details
        internal string GetStudentParentDetails(PrintOptions options = null, string courseId = "", string section = "")
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            if (options == null) options = new PrintOptions() { RecordsPerPage = 1 };
            if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;

            //model
            var model = GetStudentParentDetailsModel(courseId, section);

            //prepare report_data based on model & report options
            var title1 = $"Student Details";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);
            var report_data = GetReportData_StudentParentDetails(inst_dto, model, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }

        internal List<StudentParentDetailsRptDto> GetStudentParentDetailsModel(string courseId = "", string section = "")
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<StudentParentDetailsRptDto>();
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);

            var stu_list = _studentQry.GetAll(courseId, section);

            var group = stu_list
                            .GroupBy(o => new { o.CourseId, o.CourseName, o.SectionName })
                            .Select(g => new
                            {
                                CourseId = g.Key.CourseId,
                                CourseName = g.Key.CourseName,
                                Section = g.Key.SectionName,
                                Students = g.ToList()
                            });
            foreach (var item in group)
            {
                var dto = new StudentParentDetailsRptDto()
                {
                    InstId = inst_id,
                    CourseId = item.CourseId,
                    CourseName = item.CourseName,
                    SectionName = item.Section
                };
                var students = item.Students.OrderBy(o => o.Name);
                foreach (var stu in students)
                {
                    if (stu == null || stu.EnrollmentStatus != EnrollmentStatusEnum.Active) continue;

                    var m_list = stu.Mobiles.Split(",").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                    var mobiles = String.Join(",", m_list);

                    var e_list = stu.Emails.Split(",").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                    var emails = String.Join(",", e_list);

                    var stu_dto = new StudentParentDetailsDto()
                    {
                        Id = stu.Id,
                        StudentId = stu.StudentId,
                        StudentName = Helper.DeduplicatePattern(stu.Name),
                        FatherName = Helper.DeduplicatePattern(stu.FatherName),
                        MotherName = Helper.DeduplicatePattern(stu.MotherName),
                        Mobiles = mobiles,
                        Emails = emails
                    };
                    dto.StudentList.Add(stu_dto);
                }
                result.Add(dto);
            }
            return result;
        }

        public SPD_CSV GetStudentParentDetailsAsCSV(string courseId = "", string section = "")
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new SPD_CSV();
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);

            result.Columns = "StudentId,StudentName,FatherName,MotherName,Mobiles,Emails";

            var stu_list = _studentQry.GetAll(courseId, section);

            var group = stu_list
                            .GroupBy(o => new { o.CourseId, o.CourseName, o.SectionName })
                            .Select(g => new
                            {
                                CourseId = g.Key.CourseId,
                                CourseName = g.Key.CourseName,
                                Section = g.Key.SectionName,
                                Students = g.ToList()
                            });
            foreach (var item in group)
            {
                var dto = new StudentParentDetailsRptDto()
                {
                    InstId = inst_id,
                    CourseId = item.CourseId,
                    CourseName = item.CourseName,
                    SectionName = item.Section
                };
                var students = item.Students.OrderBy(o => o.Name);
                foreach (var stu in students)
                {
                    if (stu == null || stu.EnrollmentStatus != EnrollmentStatusEnum.Active) continue;

                    var m_list = stu.Mobiles.Split(",").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                    var mobiles = String.Join(",", m_list);

                    var e_list = stu.Emails.Split(",").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                    var emails = String.Join(",", e_list);

                    StringBuilder sb = new StringBuilder();
                    sb.Append("\"'");
                    sb.Append(mobiles);
                    sb.Append("\'\"");

                    StringBuilder sb2 = new StringBuilder();
                    sb2.Append("\"");
                    sb2.Append(emails);
                    sb2.Append("\"");

                    var stu_dto = new SPD_Dto()
                    {
                        StudentId = stu.StudentId,
                        StudentName = Helper.DeduplicatePattern(stu.Name),
                        FatherName = Helper.DeduplicatePattern(stu.FatherName),
                        MotherName = Helper.DeduplicatePattern(stu.MotherName),
                        Mobiles = sb.ToString(),
                        Emails = sb2.ToString()
                    };
                    result.Students.Add(stu_dto);
                }

            }

            return result;
        }

        internal object GetReportData_StudentParentDetails(ClientDto inst, List<StudentParentDetailsRptDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }

        // Students Sectionwise Breakup
        internal string GetSectionwiseBreakUp(PrintOptions options = null, string courseId = "", string section = "")
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            if (options == null) options = new PrintOptions() { RecordsPerPage = 1 };
            if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;

            //model
            var model = GetSectionwiseBreakUpModel(courseId, section);

            //prepare report_data based on model & report options
            var title1 = $"Sectionwise Students Breakup";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);
            var report_data = GetReportData_SectionwiseStudent(inst_dto, model, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }
        internal List<SectionwiseStudentDto> GetSectionwiseBreakUpModel(string courseId = "", string section = "")
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<SectionwiseStudentDto>();
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);

            var stu_list = _studentQry.GetAll(courseId, section);

            var group = stu_list
                            .GroupBy(o => new { o.CourseId, o.CourseName, o.SectionName })
                            .Select(g => new
                            {
                                CourseId = g.Key.CourseId,
                                CourseName = g.Key.CourseName,
                                Section = g.Key.SectionName,
                                Students = g.ToList()
                            });
            foreach (var item in group)
            {
                var dto = new SectionwiseStudentDto()
                {
                    InstId = inst_id,
                    CourseId = item.CourseId,
                    CourseName = item.CourseName,
                    SectionName = item.Section
                };
                foreach (var stu in item.Students)
                {
                    if (stu == null || stu.EnrollmentStatus != EnrollmentStatusEnum.Active) continue;

                    var stu_dto = new SectionwiseStudentDetailsDto()
                    {
                        Id = stu.Id,
                        StudentId = stu.StudentId,
                        StudentName = stu.Name,
                        StudentGender = stu.Gender.HasValue ? stu.Gender.Value.ToString() : GenderEnum.NoInfo.ToString(),
                        EnrollmentStatus = stu.EnrollmentStatus.ToString(),
                        RollNo = stu.RollNo,
                        Email = stu.Emails,
                        Mobile = stu.Mobiles
                    };
                    dto.StudentList.Add(stu_dto);
                }
                result.Add(dto);
            }
            return result;
        }
        internal object GetReportData_SectionwiseStudent(ClientDto inst, List<SectionwiseStudentDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }

        // Students Coursewise Breakup
        internal string GetCoursewiseBreakUp(string courseId = "all")
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            // if (options == null) options = new PrintOptions() { RecordsPerPage = 3 };
            // if (options.RecordsPerPage == 0) options.RecordsPerPage = 3;
            var options = _policyEngine.GetPrintOptions();

            //model
            var model = GetCoursewiseBreakUpModel(courseId);

            //prepare report_data based on model & report options
            var title1 = $"Coursewise Students Breakup";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);
            var report_data = GetReportData_CoursewiseBreakUp(inst_dto, model, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }

        internal List<CoursewiseStudentDto> GetCoursewiseBreakUpModel(string courseId = "all")
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<CoursewiseStudentDto>();

            var qry = _session
                            .Query<Student_Coursewise_Count.Result>()
                            .Where(o => o.InstId == inst_id);

            if (!string.IsNullOrEmpty(courseId) && (!courseId.Equals("all", StringComparison.InvariantCultureIgnoreCase)))
                qry = qry.Where(o => o.CourseId.Equals(courseId));

            var stus = qry.ToList();

            var group = stus
                            .GroupBy(o => new { o.CourseId, o.CourseName })
                            .Select(g => new
                            {
                                CourseId = g.Key.CourseId,
                                CourseName = g.Key.CourseName,
                                Sections = g.GroupBy(s => new { s.SectionName })
                                .Select(b => new
                                {
                                    Section = b.Key.SectionName,
                                    Boys = b.First().Boys,
                                    Girls = b.First().Girls,
                                })
                            });

            foreach (var c in group)
            {
                var dto = new CoursewiseStudentDto()
                {
                    InstId = inst_id,
                    CourseId = c.CourseId,
                    CourseName = c.CourseName
                };
                var _sections = c.Sections.OrderBy(o => o.Section).ToList();
                foreach (var s in _sections)
                {
                    var sec_dto = new SectionDetail()
                    {
                        SectionName = s.Section,
                        NumberOfBoys = s.Boys,
                        NumberOfGirls = s.Girls,
                    };
                    dto.Sections.Add(sec_dto);
                }
                result.Add(dto);
            }

            result = result.OrderByDescending(o => Enumerable.Reverse(_policyEngine.CourseOrder()).ToList().IndexOf(o.CourseName)).ToList();
            return result;
        }
        internal object GetReportData_CoursewiseBreakUp(ClientDto inst, List<CoursewiseStudentDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }

        // Basic Sectionwise Students Breakup
        internal string GetBasicSectionwiseBreakUp(PrintOptions options = null, string courseId = "all", string section = "all", bool newAdmissionsOnly = false)
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            if (options == null) options = new PrintOptions() { RecordsPerPage = 1 };
            if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;

            //model
            var model = GetBasicSectionwiseBreakUpModel(courseId, section, newAdmissionsOnly);

            //prepare report_data based on model & report options
            var title1 = $"Sectionwise Students Breakup";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);
            var report_data = GetReportData_BasicSectionwiseStudent(inst_dto, model, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }

        private bool isNewStudent(string studentId, DateTime doa, DateTime acadYearFrom, DateTime acadYearTo, string coursePrefix)
        {
            // if doa not available => regex match studentId
            if (doa.Equals(DateTime.MinValue))
            {
                var _from = acadYearFrom.ToString("yy");
                var pattern = $"^{_from}{coursePrefix}";

                Regex r = new Regex(pattern);
                Match m = r.Match(studentId);
                if (m.Success) return true;
            }
            // else => isNewStudent if doa is within acadYear
            else
            {
                if (doa >= acadYearFrom && doa <= acadYearTo) return true;
            }
            return false;
        }

        internal List<BS_StudentDto> GetBasicSectionwiseBreakUpModel(string courseId = "all", string section = "all", bool newAdmissionsOnly = false)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<BS_StudentDto>();
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);

            var stu_list = _studentQry.GetAll(courseId, section);

            if (stu_list.Count == 0) throw new ArgumentNullException(nameof(stu_list));

            var course_name = stu_list.First().CourseName;
            var course_prefix = courseId == "all" ? "." : _policyEngine.CoursePrefix(course_name);
            var inst_dto = RedisCache.GetClientDto(inst_id);
            var _from = inst_dto.AcademicYearFrom.Value.ToString("yy");

            var group = stu_list
                            .OrderByDescending(o => Enumerable.Reverse(_policyEngine.CourseOrder()).ToList().IndexOf(o.CourseName))
                            .ThenBy(o => o.SectionName)
                            .ThenBy(o => o.Name)
                            .GroupBy(o => new { o.CourseId, o.CourseName, o.SectionName })
                            .Select(g => new
                            {
                                CourseId = g.Key.CourseId,
                                CourseName = g.Key.CourseName,
                                Section = g.Key.SectionName,
                                Students = g.ToList()
                            });
            foreach (var item in group)
            {
                var dto = new BS_StudentDto()
                {
                    InstId = inst_id,
                    CourseId = item.CourseId,
                    CourseName = item.CourseName,
                    SectionName = item.Section
                };
                foreach (var stu in item.Students)
                {
                    if (stu == null || stu.EnrollmentStatus != EnrollmentStatusEnum.Active) continue;

                    if (newAdmissionsOnly)
                    {
                        if (string.IsNullOrEmpty(stu.StudentId)) continue;
                        var newStudent = isNewStudent(stu.StudentId, stu.DOA, inst_dto.AcademicYearFrom.Value, inst_dto.AcademicYearTo.Value, course_prefix);
                        if (!newStudent) continue;
                    }

                    var student_id = get_student_id(stu.StudentId);

                    var stu_dto = new BS_StudentDetailsDto()
                    {
                        Id = stu.Id,
                        StudentId = (string.IsNullOrEmpty(student_id) && stu.Tags.Contains("RTE")) ? "RTE" : student_id,
                        StudentName = stu.Name,
                        EnrollmentStatus = stu.EnrollmentStatus.ToString(),
                        StudentGender = stu.GenderAsString
                    };
                    dto.StudentList.Add(stu_dto);
                }
                result.Add(dto);
            }
            return result;
        }
        private string get_student_id(string studentId)
        {
            Guid.TryParse(studentId, out Guid guid);
            return guid == Guid.Empty ? studentId : "";
        }

        internal object GetReportData_BasicSectionwiseStudent(ClientDto inst, List<BS_StudentDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }

        // New admissions report
        internal string GetNewAdmissions(List<string> courseIds, PrintOptions options = null)
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            if (options == null) options = new PrintOptions() { RecordsPerPage = 40 };
            if (options.RecordsPerPage == 0) options.RecordsPerPage = 40;

            //model
            var model = GetNewAdmissionsModel(courseIds);

            //prepare report_data based on model & report options
            var title1 = $"New Admissions {inst_dto.AcademicYearFrom.Value.Year}-{inst_dto.AcademicYearTo.Value.Year}";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);
            var report_data = GetReportData_NewAdmissions(inst_dto, new List<NewAdmissionsDto>() { model }, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }

        internal NewAdmissionsDto GetNewAdmissionsModel(List<string> course_ids = null)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new NewAdmissionsDto();
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);

            var stu_list = _studentQry.GetAll();

            if (stu_list.Count == 0) throw new ArgumentNullException(nameof(stu_list));

            var course_prefix = ".";
            var inst_dto = RedisCache.GetClientDto(inst_id);
            var _from = inst_dto.AcademicYearFrom.Value.ToString("yy");

            //if (course_ids != null) stu_list = stu_list.Where(o => o.CourseId.In(course_ids)).ToList();

            var students = stu_list
                            .OrderByDescending(o => Enumerable.Reverse(_policyEngine.CourseOrder()).ToList().IndexOf(o.CourseName))
                            .ThenBy(o => o.SectionName)
                            .ThenBy(o => o.Name)
                            .ToList();

            foreach (var stu in students)
            {
                if (stu == null || stu.EnrollmentStatus != EnrollmentStatusEnum.Active) continue;
                if (course_ids != null && course_ids.Count > 0 && !stu.CourseId.In(course_ids)) continue;

                if (string.IsNullOrEmpty(stu.StudentId)) continue;
                var newStudent = isNewStudent(stu.StudentId, stu.DOA, inst_dto.AcademicYearFrom.Value, inst_dto.AcademicYearTo.Value, course_prefix);
                if (!newStudent) continue;

                var student_id = get_student_id(stu.StudentId);

                var stu_dto = new NewAdmission_StudentDetailsDto()
                {
                    Id = stu.Id,
                    StudentId = (string.IsNullOrEmpty(student_id) && stu.Tags.Contains("RTE")) ? "RTE" : student_id,
                    StudentName = stu.Name,
                    EnrollmentStatus = stu.EnrollmentStatus.ToString(),
                    StudentGender = stu.GenderAsString,
                    CourseId = stu.CourseId,
                    CourseName = stu.CourseName,
                    SectionName = stu.SectionName,
                    InstId = stu.InstId
                };
                result.StudentList.Add(stu_dto);
            }
            return result;
        }

        internal object GetReportData_NewAdmissions(ClientDto inst, List<NewAdmissionsDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }

        // Absentees report
        internal string GetAbsenteesReport(DateTime date, PrintOptions options = null)
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            if (options == null) options = new PrintOptions() { RecordsPerPage = 1 };
            if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;

            //model
            var model = GetAbsenteesReportModel(date);

            //prepare report_data based on model & report options
            var title1 = $"Absentees List";
            var title2 = $"Academic Year {inst_dto.AcademicYearFrom.Value.Year} - {inst_dto.AcademicYearTo.Value.Year}";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, title2, string.Empty, string.Empty);
            var report_data = GetReportData_AbsenteesReport(inst_dto, new List<AbsenteesReportDto>() { model }, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }
        internal AbsenteesReportDto GetAbsenteesReportModel(DateTime date)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var absentees = _studentQry.GetAbsenteesOn(inst_id, date);

            var model = new AbsenteesReportDto()
            {
                InstId = inst_id,
                AttendanceDate = date
            };
            var stus = RedisCache.GetStudentDetails(inst_id, true);

            foreach (var de in absentees)
            {
                foreach (var sId in de.Value)
                {
                    if (string.IsNullOrEmpty(sId)) continue;

                    var stu_dto = stus.Find(s => s.Id.Equals(sId));
                    if (stu_dto == null) continue;
                    if (model.StudentList.FirstOrDefault(o => o.StudentId == sId) != null) continue;
                    var stud = new AbsentStudentDto()
                    {
                        CourseId = stu_dto.CourseId,
                        Course = stu_dto.CourseNameAndBranch,
                        Section = stu_dto.SectionName,
                        StudentId = stu_dto.Id,
                        StudentName = stu_dto.Name
                    };
                    model.StudentList.Add(stud);
                }
            }
            return model;
        }
        internal object GetReportData_AbsenteesReport(ClientDto inst, List<AbsenteesReportDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }


        internal string GetTCStudentsList(List<string> courseIds = null, PrintOptions options = null)
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            if (options == null) options = new PrintOptions() { RecordsPerPage = 1 };
            if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;

            //model
            var model = GetTCStudentsModel(courseIds);

            //prepare report_data based on model & report options
            var title1 = $"TC Students List";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, string.Empty, string.Empty, string.Empty);
            var report_data = GetReportData_TCStudents(inst_dto, model, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }

        internal List<StudentDetailsRptDto> GetTCStudentsModel(List<string> course_ids = null)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<StudentDetailsRptDto>();
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);
            Expression<Func<Student_Inst_Index.Result, bool>> filterByTCStudents = o => o.EnrollmentStatus == EnrollmentStatusEnum.TC;
            Expression<Func<Student_Inst_Index.Result, bool>> filterByCourses = o => o.CourseId.In(course_ids);

            var qry = _session.Query<Student_Inst_Index.Result>();
            qry = qry.Where(filterByInst);
            qry = qry.Where(filterByTCStudents);
            qry = course_ids != null && course_ids.Count > 0 ? qry.Where(filterByCourses) : qry;

            var students = qry.OfType<Rpt_StudentDto>().ToList();
            var dto = new StudentDetailsRptDto();
            foreach (var stu in students)
            {
                if (stu == null) continue;
                var enrl = stu.Enrollments.Find(o => o.InstId == inst_id);

                var stu_dto = new StudentDetailsDto()
                {
                    Id = stu.Id,
                    StudentId = enrl.StudentId,
                    FirstName = stu.FirstName,
                    LastName = stu.LastName,
                    DOB = stu.DOB.HasValue ? stu.DOB.Value.ToString("dd-MM-yyyy") : string.Empty,
                    CourseName = enrl.CourseName,
                    Section = enrl.SectionName,
                    StudentGender = stu.Gender
                };
                dto.StudentList.Add(stu_dto);
            }
            dto.StudentList = dto.StudentList
                                        .OrderByDescending(o => Enumerable.Reverse(_policyEngine.CourseOrder()).ToList().IndexOf(o.CourseName))
                                        .ThenBy(o => o.Section)
                                        .ThenBy(o => o.FirstName)
                                        .ToList();
            result.Add(dto);
            return result;
        }
        internal object GetReportData_TCStudents(ClientDto inst, List<StudentDetailsRptDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }

        internal string GetIEP(string studentId, PrintOptions options = null)
        {
            //context
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            //report header
            var inst_dto = RedisCache.GetClientDto(inst_id);

            //report options
            if (options == null) options = new PrintOptions() { RecordsPerPage = 1 };
            if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;

            //model
            var model = GetIEPModel(studentId);

            //prepare report_data based on model & report options
            var title1 = $"";
            var title2 = $"";
            Tuple<string, string, string, string> title = Tuple.Create<string, string, string, string>(title1, title2, string.Empty, string.Empty);
            var report_data = GetReportData_IEPReport(inst_dto, new List<IEPReportDto>() { model }, options, title);

            //get template
            var template_string = template_service.GetTemplate();
            var template = TemplateService.GetTemplateInstance(template_string);

            //render
            template.Add("m", report_data);
            var result = template.Render();
            return result;
        }

        internal IEPReportDto GetIEPModel(string studentId)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var student = _session.Load<Student>(studentId);
            if (student == null) throw new ObjectNotFoundException();

            var result = new IEPReportDto();
            result.Id = studentId;
            result.Name = student.Name;
            result.IEP = student.IEP;
            result.DOB = student.DOB;
            result.Age = student.DOB.HasValue ? CalculateAge(student.DOB.Value) : string.Empty;
            return result;

        }
        internal object GetReportData_IEPReport(ClientDto inst, List<IEPReportDto> model, PrintOptions options, Tuple<string, string, string, string> title)
        {
            options = options == null ? new PrintOptions() { } : options;
            var report = new ReportDto(options);
            var chunk_size = report.Options.RecordsPerPage;
            var iter_indx = 0;
            var title1 = title.Item1;
            var title2 = title.Item2;
            var title3 = title.Item3;
            var title4 = title.Item4;
            foreach (var chunk in model.Chunk(chunk_size))
            {
                var page = new Page(options.PrintBorder);
                page.SetHeader(inst, iter_indx, report.Options);
                page.SetTitle(title1, title2, title3, title4, iter_indx, report.Options);
                page.PC = chunk;
                page.SetFooter(inst, iter_indx, report.Options);
                report.Pages.Add(page);
                iter_indx++;
            }
            //serialize & de-serialize to ensure that string-template gets the model correctly.
            var rpt_str = JsonHelper.Serialize(report);
            var obj = JsonHelper.Deserialize(rpt_str);
            return obj;
        }

        public string CalculateAge(DateTime dob)
        {
            DateTime today = DateTime.Today;

            int years = today.Year - dob.Year;
            int months = today.Month - dob.Month;

            if (today.Day < dob.Day)
            {
                months--;
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            var monthStr = months == 1 || months == 0 ? "month" : "months";
            var result = $"{years} years and {months} {monthStr}";
            return result;
        }

    }
}