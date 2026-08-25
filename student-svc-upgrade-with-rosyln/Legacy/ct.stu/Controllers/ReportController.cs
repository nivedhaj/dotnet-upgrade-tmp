using ct._Core.Repo.Dto;
using ct.stu.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Repo.Dto;
using System;
using System.Collections.Generic;

namespace ct.Controllers
{
    /// <summary>
    /// Report Controller
    /// </summary>
    [Route("api/stu/[controller]")]
    public class ReportController : Controller
    {
        private readonly ReportService report_service;
        private readonly ILogger<ReportController> _logger;
        private readonly CsvService _csvService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="reportService"></param>
        /// <param name="csvService"></param>
        public ReportController(ILogger<ReportController> logger, ReportService reportService, CsvService csvService)
        {
            _logger = logger;
            report_service = reportService;
            _csvService = csvService;
        }

        /// <summary>
        /// Marks Card Report
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("getMarksCard")]
        public IActionResult GetMarksCardReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }
            var testData = false;
            try { testData = meta.TestData; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetMarksCard(options, testData);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Ok(_data);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// Remder Template
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("renderTemp")]
        public IActionResult RenderTempReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }
            var testData = false;
            try { testData = meta.TestData; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.RenderTemp(options, testData);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Ok(_data);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// Fee Receipt
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("viewFeeReceipt")]
        public IActionResult ViewFeeReceiptReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }
            var testData = false;
            try { testData = meta.TestData; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.ViewFeeReceipt(options, testData);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Ok(_data);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// View TC
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("viewTC")]
        public IActionResult ViewTCReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }
            var testData = false;
            try { testData = meta.TestData; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.ViewTC(options, testData);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Ok(_data);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// Certificate
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("certificate")]
        public IActionResult CertificateReport([FromBody] CertificateDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.ContentType))
            {
                var errResponse = new ApiResponse().Create400();
                return Json(errResponse);
            }
            if (dto.ContentType.Equals("NoDue", StringComparison.InvariantCultureIgnoreCase))
            {
                var _data = report_service.GetNoDueCertificate(dto);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            else
            {
                var resp = new ApiResponse().Create400();
                return Json(resp);
            }
        }

        /// <summary>
        /// Marks For all students
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("getMarksForAllStudentsInExam")]
        public IActionResult GetMarksForAllStudentsInExamReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }
            var testData = false;
            try { testData = meta.TestData; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetMarksForAllStudentsInExam(options, testData);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Ok(_data);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// Todays timetable
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("getStudentTodayTimetableReport")]
        public IActionResult GetStudentTodayTimetableReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }
            var testData = false;
            try { testData = meta.TestData; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetStudentTodayTimetable(options, testData);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Ok(_data);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// Hall ticket
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("hallTicket")]
        public IActionResult HallTicketReport([FromBody] HallTicketDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.ContentType))
            {
                var errResponse = new ApiResponse().Create400();
                return Json(errResponse);
            }
            if (dto.ContentType.Equals("HallTicket", StringComparison.InvariantCultureIgnoreCase))
            {
                var _data = report_service.GetHallTicket(dto);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            else
            {
                var resp = new ApiResponse().Create400();
                return Json(resp);
            }
        }

        /// <summary>
        /// Student breakup - Sectionwise
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("studentDetails")]
        public IActionResult StudentDetailsReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }

            var data = dObj.Data;

            var course_ids = new List<string>();
            try { course_ids = ((JArray)data.CourseIds).ToObject<List<string>>(); } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetStudentDetails(options, course_ids);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        [HttpGet("studentDetailsCSV")]
        public IActionResult GetStudentDetailsCSV([FromBody] List<string> courseIds)
        {
            var data = report_service.GetStudentDetailsModel(courseIds);
            var csv = _csvService.GetCSV(data);
            var result = new DataDto() { CSV = csv, FileName = $"Student Details" };
            var response = new ApiResponse().Create200().WithMessage($"Retrieved Student details").WithData(result);
            return Json(response);
        }

        /// <summary>
        /// Student breakup - Sectionwise
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("sectionwiseBreakUp")]
        public IActionResult SectionwiseBreakUp([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }

            var data = dObj.Data;
            var courseId = "";
            var section = "";

            try { courseId = data.CourseId; } catch (RuntimeBinderException) { }
            try { section = data.Section; } catch (RuntimeBinderException) { }

            try
            {
                var _data = report_service.GetSectionwiseBreakUp(options, courseId, section);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// Student breakup - Sectionwise (basic)
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("basicSectionwiseBreakUp")]
        public IActionResult BasicSectionwiseBreakUp([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }

            var data = dObj.Data;
            bool newAdmissionsOnly = false;
            var courseId = "all";
            var section = "all";

            try { courseId = data.CourseId; } catch (RuntimeBinderException) { }
            try { section = data.Section; } catch (RuntimeBinderException) { }
            try { newAdmissionsOnly = data.NewAdmissionsOnly; } catch (RuntimeBinderException) { }

            try
            {
                var _data = report_service.GetBasicSectionwiseBreakUp(options, courseId, section, newAdmissionsOnly);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// New Admissions Report
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("newAdmissions")]
        public IActionResult NewAdmissions([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            var data = dObj.Data;
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }
            var course_ids = new List<string>();
            try { course_ids = ((JArray)data.CourseIds).ToObject<List<string>>(); } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetNewAdmissions(course_ids, options);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        /// <summary>
        /// Coursewise BreakUp
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("coursewiseBreakUp")]
        public IActionResult CoursewiseBreakUp([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;

            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); }
            catch (RuntimeBinderException) { }

            var data = dObj.Data;
            string courseId = data.CourseId;
            try
            {
                var result = report_service.GetCoursewiseBreakUp(courseId);
                var resp = new ApiResponse().Create200().WithData(result);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var resp = new ApiResponse().Create400().WithMessage(ex.Message);
                return Json(resp);
            }
        }

        [HttpPost("coursewiseBreakUpAsJson")]
        public IActionResult CoursewiseBreakUpAsJson()
        {
            var courseId = "all";
            try
            {
                var result = report_service.GetCoursewiseBreakUpModel(courseId);
                var resp = new ApiResponse().Create200().WithData(result);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var resp = new ApiResponse().Create400().WithMessage(ex.Message);
                return Json(resp);
            }
        }

        /// <summary>
        /// Absentees report
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("absentees")]
        public IActionResult AbsenteesReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); }
            catch (RuntimeBinderException) { }

            var data = dObj.Data;
            DateTime date = data.Date;
            try
            {
                var result = report_service.GetAbsenteesReport(date, options);
                var resp = new ApiResponse().Create200().WithData(result);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var resp = new ApiResponse().Create400().WithMessage(ex.Message);
                return Json(resp);
            }
        }

        /// <summary>
        /// Student parent details
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("studentParentDetails")]
        public IActionResult StudentParentDetailsReport([FromBody] JObject jObj)
        {

            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }

            var data = dObj.Data;
            var courseId = "";
            var section = "";

            try { courseId = data.CourseId; } catch (RuntimeBinderException) { }
            try { section = data.Section; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetStudentParentDetails(options, courseId, section);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }
        /// <summary>
        /// Student parent details as csv
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns></returns>
        [HttpPost("studentParentDetailsCSV")]
        public IActionResult StudentParentDetailsCSV([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;

            var data = dObj.Data;
            var courseId = "";
            var section = "";

            try { courseId = data.CourseId; } catch (RuntimeBinderException) { }
            try { section = data.Section; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetStudentParentDetailsAsCSV(courseId, section);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        [HttpPost("tcStudents")]
        public IActionResult TCStudentsReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            var data = dObj.Data;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }

            var course_ids = new List<string>();
            try { course_ids = ((JArray)data.CourseIds).ToObject<List<string>>(); } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetTCStudentsList(course_ids, options);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }

        [HttpPost("iep")]
        public IActionResult IEPReport([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var meta = dObj.Meta;
            PrintOptions options = new PrintOptions();
            try { options = meta.PrintOptions.ToObject<PrintOptions>(); } catch (RuntimeBinderException) { }

            var data = dObj.Data;
            var studentId = "";

            try { studentId = data.StudentId; } catch (RuntimeBinderException) { }
            try
            {
                var _data = report_service.GetIEP(studentId, options);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(errResp);
            }
        }




    }
}