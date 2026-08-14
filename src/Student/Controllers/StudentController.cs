using ct._Core.Repo.Dto;
using ct._Core.Repo.Resource;
using ct._Core.Util;
using ct.stu.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Repo.Db.Command;
using Repo.Db.Index;
using Repo.Db.Query;
using Repo.Dto;
using Repo.Resource;
using System;
using System.Collections.Generic;

namespace ct.Controllers
{
    /// <summary>
    /// Student management methods
    /// </summary>
    [Route("api/stu/[controller]")]
    public class StudentController : Controller
    {
        // private CoudinaryService _cs;
        private readonly ILogger<StudentController> _logger;
        private readonly StudentCmd _studentCmd;
        private readonly StudentQry _studentQry;
        private readonly StudentService _studentService;
        private readonly CsvService _csvService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="studentCmd"></param>
        /// <param name="studentQry"></param>
        /// <param name="studentService"></param>
        /// <param name="csvService"></param>
        public StudentController(ILogger<StudentController> logger, StudentCmd studentCmd, StudentQry studentQry, StudentService studentService, CsvService csvService)
        {
            _logger = logger;
            _studentCmd = studentCmd;
            _studentQry = studentQry;
            _studentService = studentService;
            _csvService = csvService;
        }

        /// <summary>
        /// Get Students
        /// </summary>
        /// <param name="id"></param>
        /// <param name="activeStudentsOnly"></param>
        /// <param name="todaysAbsenteesOnly"></param>
        /// <param name="tcStudentsOnly"></param>
        /// <param name="per"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(
            [FromQuery] string id = "",
            [FromQuery] bool activeStudentsOnly = false,
            [FromQuery] bool todaysAbsenteesOnly = false,
            [FromQuery] bool tcStudentsOnly = false,
            [FromQuery] int per = 0
            )
        {
            if (string.IsNullOrEmpty(id))
            {
                var result = _studentQry.Get<Student_Inst_Index.Result>(activeStudentsOnly, todaysAbsenteesOnly, tcStudentsOnly: tcStudentsOnly, per: per);
                var resp = new ApiResponse().Create200().WithMessage("Retrieved students").WithData(result.Data).WithTotalPages(result.TotalPages).WithRecordsPerPage(result.RecordsPerPage).WithTotalRecords(result.TotalRecords);
                return Json(resp);
            }
            else
            {
                var _data = _studentQry.GetById(id);
                var resp = new ApiResponse().Create200().WithData(_data);
                return Json(resp);
            }
        }

        /// <summary>
        /// Get All Students From Cache. Excludes RTE by default.
        /// </summary>
        /// <param name="includeRTE">Defaults to false</param>
        /// <returns></returns>
        [HttpGet("all")]
        public IActionResult All([FromQuery] bool includeRTE = false)
        {
            var result = _studentQry.GetAllFromCache(includeRTE);
            var resp = new ApiResponse().Create200().WithMessage($"Retrieved students. RTE included: {includeRTE}").WithData(result);
            return Json(resp);
        }

        /// <summary>
        /// Create Student
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] CreateStudentDto json)
        {
            var result = _studentCmd.Create(
                json.AadharNumber,
                json.AdmissionNumber,
                json.ApplicationNumber,
                json.CourseName,
                json.CourseId,
                json.DOA,
                json.DOB,
                json.Email,
                json.Gender,
                json.Mobile,
                json.Name,
                json.SecName,
                json.StudentId,
                json.TermName,
                json.Branch,
                json.InstId,
                json.Id
            );
            return CreatedAtRoute(new { id = result.Id }, result);
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Route("uploadPhoto")]
        [HttpPost]
        public IActionResult UploadPhoto([FromQuery] string fileName)
        {
            var result = _studentCmd.UploadPhoto(fileName);
            return Ok("To Do");
            // return Json(result);
        }

        /// <summary>
        /// Enable
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("enable")]
        public IActionResult Enable([FromQuery] string id)
        {
            _studentCmd.Enable(id);
            return Json(id);
        }
        /// <summary>
        /// Set Student Status to Disabled
        /// </summary>
        /// <param name="id"></param>
        [HttpPost("disable")]
        public IActionResult Disable([FromQuery] string id)
        {
            _studentCmd.Disable(id);
            return Json(id);
        }

        /// <summary>
        /// Sets the Status to TC. No other information is captured
        /// TcStudentDto is reserved for futire use.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("tc")]
        public IActionResult TC([FromQuery] string id)
        {
            _studentCmd.TC(id);
            return Json(id);
        }

        /// <summary>
        /// Assign Student Id
        /// </summary>
        /// <param name="json">AssignStudentIdDto</param>
        /// <returns></returns>
        [HttpPost("AssignStudentId")]
        public IActionResult AssignStudentId([FromBody] AssignStudentIdDto json)
        {
            var student_id = _studentCmd.AssignStudentId(json.Id, json.StudentId);
            var resp = new ApiResponse().Create200().WithMessage("Assigned StudentId").WithData(student_id);
            return Json(resp);
        }

        /// <summary>
        /// Student Promotion
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("promote")]
        public IActionResult Promote([FromBody] StuPromotionDto dto)
        {
            try
            {
                string id = dto.StudentId;
                string baseInstId = dto.BaseInstId;
                string baseCourseId = dto.BaseCourseId;
                string baseTerm = dto.BaseTerm;
                string baseSection = dto.BaseSection;

                string targetInstId = dto.TargetInstId;
                string targetCourseId = dto.TargetCourseId;
                string targetTerm = dto.TargetTerm;
                string targetSection = dto.TargetSection;

                _studentCmd.Promote(id, baseInstId, baseCourseId, baseTerm, baseSection, targetInstId, targetCourseId, targetTerm, targetSection);
                var resp = new ApiResponse().Create200().WithMessage("Student promoted");
                return Json(resp);
            }
            catch (Exception ex)
            {
                var resp = new ApiResponse().Create400().WithMessage(ex.Message);
                return Json(resp);
            }
        }

        /// <summary>
        /// Home Address set
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("setHomeAddress")]
        public IActionResult SetHomeAddress([FromBody] SetHomeAddressStudentDto json)
        {
            var result = _studentCmd.SetHomeAddress(json.Id, json.City, json.Country, json.Line1, json.Line2, json.Pincode, json.State);
            return Json(result);
        }

        /// <summary>
        /// Home phone set
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("setHomePhone")]
        public IActionResult SetHomePhone([FromBody] HomePhoneDto json)
        {
            // Console.WriteLine(homePhone);
            var result = _studentCmd.SetHomePhone(json.Id, json.HomePhone);
            return Json(result);
        }


        /// <summary>
        /// Set Primary Email
        /// </summary>
        /// <returns></returns>
        [HttpPost("setPrimaryEmail")]
        public IActionResult SetPrimaryEmail([FromBody] SetPrimaryEmailDto json)
        {
            var result = _studentCmd.SetPrimaryEmail(json.Id, json.Email);
            return Json(result);
        }

        /// <summary>
        /// Set Primary Mobile
        /// </summary>
        /// <returns></returns>
        [HttpPost("setPrimaryMobile")]
        public IActionResult SetPrimaryMobile([FromBody] SetPrimaryMobileDto json)
        {
            var result = _studentCmd.SetPrimaryMobile(json.Id, json.Mobile);
            return Json(result);
        }


        /// <summary>
        /// Set Tags
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("updateTags")]
        public IActionResult SetTags([FromBody] UpdateTagsDto json)
        {
            var result = _studentCmd.SetTags(json.Id, json.Tags);
            var resp = new ApiResponse().Create200().WithData(result);
            return Json(resp);
        }

        /// <summary>
        /// Update course name for a student
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("updateCourse")]
        public IActionResult UpdateCourse([FromBody] UpdateCourseDto json)
        {
            var result = _studentCmd.UpdateCourse(json.Id, json.CourseName, json.CourseId, json.Section, json.Branch);
            var resp = new ApiResponse().Create200().WithData(result);
            return Json(resp);
        }

        /// <summary>
        /// Update Gender of stu
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("updateGender")]
        public IActionResult UpdateGender([FromBody] UpdateGenderDto json)
        {
            var result = _studentCmd.UpdateGender(json.Id, json.Gender);
            var resp = new ApiResponse().Create200().WithData(result);
            return Json(resp);
        }
        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("timetable")]
        public IActionResult TimeTable([FromBody] TimeTableDto json)
        {
            //var result = _studentCmd.TimeTable(json.);
            //ToDo
            return Ok("To Do");
        }

        /// <summary>
        /// Update Father Details
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("updateFatherDetails")]
        public IActionResult UpdateFatherDetails([FromBody] UpdateFatherDetailsStudentDto json)
        {
            var result = _studentCmd.UpdateFatherDetails(json.Id, json.Email, json.Mobile, json.Name, json.PAN);
            return Json(result);
        }

        /// <summary>
        /// Update Mother details
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("updateMotherDetails")]
        public IActionResult UpdateMotherDetails([FromBody] UpdateMotherDetailsStudentDto json)
        {
            var result = _studentCmd.UpdateMotherDetails(json.Id, json.Email, json.Mobile, json.Name, json.PAN);
            return Json(result);
        }

        /// <summary>
        /// updateFatherEmail
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("updateFatherEmail")]
        public IActionResult UpdateFatherEmail([FromBody] UpdateFatherDetailsStudentDto json)
        {
            var result = _studentCmd.UpdateFatherEmail(json.Id, json.Email);
            return Json(result);
        }

        /// <summary>
        /// Update Father mobile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        [HttpPost("updateFatherMobile")]
        public IActionResult UpdateFatherMobile([FromQuery] string id, [FromQuery] string mobile)
        {
            var result = _studentCmd.UpdateFatherMobile(id, mobile);
            return Json(result);
        }

        /// <summary>
        /// Update Mother Email
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("updateMotherEmail")]
        public IActionResult UpdateMotherEmail([FromQuery] string id, [FromQuery] string email)
        {
            var result = _studentCmd.UpdateMotherEmail(id, email);
            return Json(result);
        }

        /// <summary>
        /// Update Mother Mobile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        [HttpPost("updateMotherMobile")]
        public IActionResult UpdateMotherMobile([FromQuery] string id, [FromQuery] string mobile)
        {
            var result = _studentCmd.UpdateMotherMobile(id, mobile);
            return Json(result);
        }

        /// <summary>
        /// Update Guardian Details
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("updateGuardianDetails")]
        public IActionResult UpdateGuardianDetails([FromBody] UpdateGuardianDetailsStudentDto json)
        {
            var result = _studentCmd.UpdateGuardianDetails(json.Id, json.Email, json.Gender, json.Mobile, json.Name);
            return Json(result);
        }

        /// <summary>
        /// Update Guardian Mobile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        [HttpPost("updateGuardianMobile")]
        public IActionResult UpdateGuardianMobile([FromQuery] string id, [FromQuery] string mobile)
        {
            var result = _studentCmd.UpdateGuardianMobile(id, mobile);
            return Json(result);
        }

        /// <summary>
        /// Update Guardian Email
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("updateGuardianEmail")]
        public IActionResult UpdateGuardianEmail([FromQuery] string id, [FromQuery] string email)
        {
            var result = _studentCmd.UpdateGuardianEmail(id, email);
            return Json(result);
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("update")]
        public IActionResult Update([FromBody] UpdateStudentDto json)
        {
            var result = _studentCmd.Update(json.AadharNumber, json.Id, json.AdmissionNo, json.ApplicationNo, json.DOA, json.DOB, json.Gender, json.Name);
            return Json(result);
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("getFilters")]
        public IActionResult GetFilters([FromBody] GetFiltersStudentDto json)
        {
            //var result = _studentCmd.GetFilters(json);
            //ToDo
            return Ok("To Do");
        }

        /// <summary>
        /// Set Roll No
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost("setRollNo")]
        public IActionResult SetRollNo([FromBody] SetRollNoDto json)
        {
            var result = _studentCmd.SetRollNo(json.Id, json.RollNo);
            return Json(result);
        }

        /// <summary>
        /// Cache 
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        [HttpPost("cache")]
        public IActionResult Cache([FromQuery] bool reset = false)
        {
            _studentQry.SetStudentCache(reset);
            return Ok();
        }

        /// <summary>
        /// Attendance Events
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        [HttpPost("applyEvents")]
        public IActionResult ApplyEvents([FromBody] List<EventLog> events)
        {
            try
            {
                _studentCmd.ApplyEvents(events);
                var resp = new ApiResponse().Create200();
                return Json(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                var resp = new ApiResponse().Create400().WithMessage(ex.Message);
                return Json(resp);
            }
        }
        /// <summary>
        /// Absentee SMS
        /// </summary>
        /// <returns></returns>
        [HttpPost("sms/absentees")]
        public IActionResult SmsAbsentees([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            var periodNo = 0;
            try { periodNo = dObj.Per; } catch (RuntimeBinderException) { }
            var sms = _studentCmd.SmsAbsentees(periodNumber: periodNo);
            return Ok();
        }

        /// <summary>
        /// Student facets
        /// </summary>
        /// <returns></returns>
        [HttpGet("facet")]
        public IActionResult Facet([FromQuery] bool includeStudents = true)
        {
            var facets = _studentQry.GetFacets(includeStudents);
            var resp = new ApiResponse().Create200().WithMessage("Retrieved student facets").WithData(facets);
            return Json(resp);
        }

        /// <summary>
        /// Send SMS to Parents / Students
        /// </summary>
        /// <returns></returns>
        [HttpPost("sms/parent")]
        public IActionResult SMSParentStudents([FromBody] RecipientPostData json)
        {
            var sms_sender_id = _studentQry.GetSMSSenderId();
            if (string.IsNullOrEmpty(sms_sender_id))
            {
                var resp = new ApiResponse().Create500().WithMessage(_Constants.SMS_SENDER_ID_NULL);
                return Json(resp);
            }

            var recipients = _studentCmd.GetSMSRecipients(json);
            if (recipients != null && recipients.Count > 0 && !string.IsNullOrEmpty(json.Text))
            {
                var message = json.Text;
                var chunks = recipients.DistinctBy(o => o.mobile).Chunk(256);
                foreach (var chunk in chunks)
                {
                    var sms = _studentCmd.SendSMS(SMSGatewayEnum.Infini, chunk, message, json.SMSRefId, sms_sender_id);
                }
                return Ok();
            }
            return BadRequest();
        }

        /// <summary>
        /// Send SMS to Parents / Students
        /// </summary>
        /// <returns></returns>
        [HttpPost("email/parent")]
        public IActionResult EmailParentStudents([FromBody] CreateEmailDto json)
        {
            var recipients = _studentCmd.GetEmailRecipients(json);
            if (recipients != null && recipients.Count > 0 && !string.IsNullOrEmpty(json.Message))
            {
                var message = json.Message;
                var chunks = recipients.Chunk(256);
                foreach (var chunk in chunks)
                {
                    var email = _studentCmd.SendEmail(json.Type, json.From, json.Subject, chunk, json.Message, json.Attachments);
                }
                return Ok();
            }
            return BadRequest();
        }

        /// <summary>
        /// Send SMS to Parents / Students
        /// </summary>
        /// <returns></returns>
        [HttpPost("smsRecipients")]
        public IActionResult SMSRecipients([FromBody] RecipientPostData json)
        {
            var recipients = _studentCmd.GetSMSRecipients(json);
            var resp = new ApiResponse().Create200().WithMessage("Retrieved SMS Recipients").WithData(recipients);
            return Json(resp);
        }

        /// <summary>
        /// Subscription recipients
        /// </summary>
        /// <returns></returns>
        [HttpPost("recipients")]
        public IActionResult SubscriptionRecipients([FromBody] RecipientPostData json)
        {
            var recipients = _studentQry.GetSubscriptionRecipients(json.Recipients);
            var resp = new ApiResponse().Create200().WithMessage("Retrieved Subscription Recipients").WithData(recipients);
            return Json(resp);
        }

        /// <summary>
        /// Get Student Contacts
        /// </summary>
        /// <returns></returns>
        [HttpPost("getContacts")]
        public IActionResult GetContacts([FromBody] JObject jObj)
        {
            dynamic dObj = jObj;
            try
            {
                var limitToCourses = dObj.LimitToCourses.ToObject<List<string>>();
                var limitToStudents = dObj.LimitToStudents.ToObject<List<string>>();
                var contacts = _studentCmd.GetContacts(limitToCourses, limitToStudents);
                var resp = new ApiResponse().Create200().WithMessage("Retrieved Student Contacts").WithData(contacts);
                return Json(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                var apiResponse = new ApiResponse().Create400().WithMessage(ex.Message);
                return BadRequest(apiResponse);
            }
        }

        /// <summary>
        /// Get Student Contacts Facet
        /// </summary>
        /// <returns></returns>
        [HttpPost("getContactsFromFacet")]
        public IActionResult GetContactsFromFacet([FromBody] RecipientsFacet Recipients)
        {
            var contacts = _studentCmd.GetContacts(Recipients);
            var resp = new ApiResponse().Create200().WithMessage("Retrieved Student Contacts from facet").WithData(contacts);
            return Json(resp);
        }

        /// <summary>
        /// Get Student Dto
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDto")]
        public IActionResult GetDto()
        {
            var list = new List<stu.Service.StudentDto>();
            var data = _studentService.GetDto();
            list.Add(data);
            var csv = _csvService.GetCSV(list);
            long timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            string filename = $"{timestamp}-Students.csv".ToLower();
            var filename_SHA1 = Helper.SHA1FromString(filename);
            var result = new DataDto() { CSV = csv, FileName = $"{filename}-{filename_SHA1}" };
            var response = new ApiResponse().Create200().WithMessage($"Retrieved Student dto").WithData(result);
            return Json(response);
        }

        /// <summary>
        /// Get Student Dto
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDtoWithData")]
        public IActionResult GetDtoWithData([FromQuery] string format = "json")
        {
            var dto_list = new List<stu.Service.StudentDto>();
            dto_list = _studentService.GetDtoWithData();
            if (format == "csv")
            {
                var csv = _csvService.GetCSV(dto_list);
                long timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
                string filename = $"{timestamp}-Students.csv".ToLower();
                var filename_SHA1 = Helper.SHA1FromString(filename);
                var result = new DataDto() { CSV = csv, FileName = $"{filename}-{filename_SHA1}" };
                var response = new ApiResponse().Create200().WithMessage($"Retrieved Student csv").WithData(result);
                return Json(response);
            }
            else
            {
                var response = new ApiResponse().Create200().WithMessage($"Retrieved Student data").WithData(dto_list);
                return Json(response);
            }
        }

        /// <summary>
        /// Get students for exam facets
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpGet("getStudentsForExamFacets")]
        public IActionResult GetStudentsForExamFacets([FromQuery] string courseId)
        {
            try
            {
                var result = _studentQry.GetStudentsForExamFacets(courseId);
                var response = new ApiResponse().Create200().WithMessage($"Retrieved students for exam facets").WithData(result);
                return Json(response);
            }
            catch (Exception ex)
            {
                var resp = new ApiResponse().Create400().WithMessage(ex.Message);
                return Json(resp);
            }
        }

        /// <summary>
        /// Absentee sms recipient count
        /// </summary>
        /// <param name="perNumber"></param>
        /// <returns></returns>
        [HttpGet("absenteesRecipientCount")]
        public IActionResult GetAbsenteesSMSRecipientCount([FromQuery] int perNumber)
        {
            var periodNo = 0;
            try { periodNo = perNumber; } catch (RuntimeBinderException) { }
            var result = _studentCmd.GetSMSRecipientDtoForAbsentees(periodNumber: periodNo);
            var resp = new ApiResponse().Create200().WithData(result.Count);
            return Json(resp);
        }

        /// <summary>
        /// Get students flat data
        /// </summary>
        /// <returns></returns>
        [HttpPost("flatData")]
        public IActionResult GetStudentsFlatData([FromBody] JObject jObj, [FromQuery] bool snakeCase = false)
        {
            dynamic dObj = jObj;
            List<string> studentIds = new List<string>();
            try { studentIds = dObj.StudentIds.ToObject<List<string>>(); } catch (RuntimeBinderException) { }
            var result = _studentQry.GetFlatStudentData(studentIds);
            if (snakeCase)
            {
                var snake_cased_result = Helper.serialize_to_snake_case(result);
                var response = new ApiResponse().Create200().WithMessage($"Retrieved students flat data").WithData(snake_cased_result);
                return Json(response);
            }
            else
            {
                var response = new ApiResponse().Create200().WithMessage($"Retrieved students flat data").WithData(result);
                return Json(response);
            }
        }

        /// <summary>
        /// Get student profile
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("profile")]
        public IActionResult Profile([FromQuery] string userEmail, [FromQuery] string id)
        {
            var result = _studentQry.GetStudentProfile(userEmail, id);
            var resp = new ApiResponse().Create200().WithMessage($"Retrieved student(s) profile.").WithData(result);
            return Json(resp);
        }

        /// <summary>
        /// Update Domicile
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("updateDomicile")]
        public IActionResult UpdateDomicile([FromBody] UpdateDomicileDto dto)
        {
            var result = _studentCmd.UpdateDomicile(dto.Id, dto.Caste, dto.Country, dto.MotherTongue, dto.State);
            return Json(result);
        }

        /// <summary>
        /// Disable father mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("disableFatherMobile")]
        public IActionResult DisableFatherMobile([FromQuery] string id)
        {
            var result = _studentCmd.DisableFatherMobile(id);
            return Json(result);
        }

        /// <summary>
        /// Disable mother mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("disableMotherMobile")]
        public IActionResult DisableMotherMobile([FromQuery] string id)
        {
            var result = _studentCmd.DisableMotherMobile(id);
            return Json(result);
        }

        /// <summary>
        /// Disable guardian mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("disableGuardianMobile")]
        public IActionResult DisableGuardianMobile([FromQuery] string id)
        {
            var result = _studentCmd.DisableGuardianMobile(id);
            return Json(result);
        }
        /// <summary>
        /// Enable father mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("enableFatherMobile")]
        public IActionResult EnableFatherMobile([FromQuery] string id)
        {
            var result = _studentCmd.EnableFatherMobile(id);
            return Json(result);
        }

        /// <summary>
        /// Enable mother mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("enableMotherMobile")]
        public IActionResult EnableMotherMobile([FromQuery] string id)
        {
            var result = _studentCmd.EnableMotherMobile(id);
            return Json(result);
        }

        /// <summary>
        /// Enable guardian mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("enableGuardianMobile")]
        public IActionResult EnableGuardianMobile([FromQuery] string id)
        {
            var result = _studentCmd.EnableGuardianMobile(id);
            return Json(result);
        }

        /// <summary>
        /// Disable father email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("disableFatherEmail")]
        public IActionResult DisableFatherEmail([FromQuery] string id)
        {
            var result = _studentCmd.DisableFatherEmail(id);
            return Json(result);
        }

        /// <summary>
        /// Disable mother email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("disableMotherEmail")]
        public IActionResult DisableMotherEmail([FromQuery] string id)
        {
            var result = _studentCmd.DisableMotherEmail(id);
            return Json(result);
        }

        /// <summary>
        /// Disable guardian email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("disableGuardianEmail")]
        public IActionResult DisableGuardianEmail([FromQuery] string id)
        {
            var result = _studentCmd.DisableGuardianEmail(id);
            return Json(result);
        }

        /// <summary>
        /// Enable father email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("enableFatherEmail")]
        public IActionResult EnableFatherEmail([FromQuery] string id)
        {
            var result = _studentCmd.EnableFatherEmail(id);
            return Json(result);
        }

        /// <summary>
        /// Enable mother email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("enableMotherEmail")]
        public IActionResult EnableMotherEmail([FromQuery] string id)
        {
            var result = _studentCmd.EnableMotherEmail(id);
            return Json(result);
        }

        /// <summary>
        /// Enable guardian email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("enableGuardianEmail")]
        public IActionResult EnableGuardianEmail([FromQuery] string id)
        {
            var result = _studentCmd.EnableGuardianEmail(id);
            return Json(result);
        }

        // Fee-receivable / opening-balance endpoints (feesReceivable, feesReceivableUpdate,
        // feesReceivableForSpanningFee, feesReceivableStatusUpdate, openingBalance) have been
        // REMOVED from this module. They now live in the Fees module, writing directly to the
        // Fees database. See Student/Views/StudentFeeSummary.cs for how this module reads
        // fee data back (via a materialized view, not a direct call into Fees).

        [HttpGet("widget/genderBreakup")]
        public IActionResult GetGenderBreakupData()
        {
            var result = _studentQry.GetGenderBreakupWidgetData();
            var resp = new ApiResponse().Create200().WithMessage($"Retrieved gender breakup widget data").WithData(result);
            return Json(resp);
        }

        [HttpGet("getStudentsByTag")]
        public IActionResult GetStudentsByTag([FromQuery] string tags)
        {
            var result = _studentQry.GetStudentsByTag(tags);
            var resp = new ApiResponse().Create200().WithMessage($"Retrieved students").WithData(result);
            return Json(resp);
        }
        /// <summary>
        /// Add IEP
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("iep")]
        public IActionResult AddIEP([FromBody] CreateIEPDto dto)
        {
            var iep = _studentCmd.AddIEP(dto.StudentId, dto.Terms);
            var resp = new ApiResponse().Create200().WithMessage("Student IEP updated");
            return Json(resp);
        }
        /// <summary>
        /// Get IEP
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("iep")]
        public IActionResult GetIEPDetails([FromQuery] string id)
        {
            var result = _studentQry.GetIEPDetails(id);
            var resp = new ApiResponse().Create200().WithMessage($"Retrieved students").WithData(result);
            return Json(resp);
        }

        [HttpGet("goalsPerformance")]
        public IActionResult GetGoalsPerformance([FromQuery] string studentId, [FromQuery] string term)
        {
            var result = _studentQry.GetGoalsPerformanceByDomain(studentId, term);
            var resp = new ApiResponse().Create200().WithMessage($"Retrieved students").WithData(result);
            return Json(resp);
        }

        /// <summary>
        /// Add student essentials
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("addStudentEssentials")]
        public IActionResult AddStudentEssentials([FromBody] CreateItemRecordDto dto)
        {
            var student_items_record_exists = _studentQry.StudentEssentialsExists(dto.StudentId);
            if (!student_items_record_exists)
            {
                var student_items = _studentCmd.AddStudentEssentials(dto.StudentId, dto.Name, dto.Date, dto.Amount);
                var resp = new ApiResponse().Create201().WithMessage("Student items record created");
                return Json(resp);
            }
            else
            {
                var student_items = _studentCmd.UpdateItemList(dto.StudentId, dto.Name, dto.Date, dto.Amount);
                var resp = new ApiResponse().Create200().WithMessage("Student items record updated");
                return Json(resp);
            }
        }
        /// <summary>
        /// Update essential item
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("updateItem")]
        public IActionResult UpdateItem([FromBody] UpdateItemDto dto)
        {
            var student_items = _studentCmd.UpdateItem(dto.Id, dto.ItemId, dto.Name, dto.Date, dto.Amount);
            var resp = new ApiResponse().Create200().WithMessage("Student items record updated");
            return Json(resp);
        }
        /// <summary>
        /// Disable essential item
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("disableItem")]
        public IActionResult DisableItem([FromBody] DisableItemDto dto)
        {
            var student_items = _studentCmd.DisableItem(dto.Id, dto.ItemId);
            var resp = new ApiResponse().Create200().WithMessage("Student items record disabled");
            return Json(resp);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost("enableItem")]
        public IActionResult EnableEssentialItem([FromBody] DisableItemDto dto)
        {
            var student_items = _studentCmd.EnableItem(dto.Id, dto.ItemId);
            var resp = new ApiResponse().Create200().WithMessage("Student items record enabled");
            return Json(resp);
        }
        /// <summary>
        /// Get student essentials
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [HttpGet("essentials")]
        public IActionResult GetStudentEssentials([FromQuery] string studentId)
        {
            var result = _studentQry.GetStudentEssentials(studentId);
            var resp = new ApiResponse().Create200().WithMessage($"Retrieved student essentials").WithData(result);
            return Json(resp);
        }

        [HttpPost("medicalDiagnosisReport")]
        public IActionResult MedicalDiagnosisReport([FromBody] DocumentDto json)
        {
            _studentCmd.SetMedicalDiagnosisReportPath(json.Id, json.Path);
            var resp = new ApiResponse().Create200().WithMessage("Student diagnosis report url added");
            return Json(resp);
        }

        [HttpPost("disabilityReport")]
        public IActionResult DisabilityReport([FromBody] DocumentDto json)
        {
            _studentCmd.SetDisabilityReportPath(json.Id, json.Path);
            var resp = new ApiResponse().Create200().WithMessage("Student disability report url added");
            return Json(resp);
        }

        [HttpPost("birthCertificate")]
        public IActionResult BirthCertificate([FromBody] DocumentDto json)
        {
            _studentCmd.SetBirthCertificatePath(json.Id, json.Path);
            var resp = new ApiResponse().Create200().WithMessage("Student birth certificate report url added");
            return Json(resp);
        }

        [HttpPost("identificationProof")]
        public IActionResult IdentificationProof([FromBody] DocumentDto json)
        {
            _studentCmd.SetIdentificationProofPath(json.Id, json.Path);
            var resp = new ApiResponse().Create200().WithMessage("Student identification proof url added");
            return Json(resp);
        }

        [HttpPost("addressProof")]
        public IActionResult AddressProof([FromBody] DocumentDto json)
        {
            _studentCmd.SetAddressProofPath(json.Id, json.Path);
            var resp = new ApiResponse().Create200().WithMessage("Student address proof url added");
            return Json(resp);
        }

        [HttpPost("therapyReport")]
        public IActionResult TherapyReport([FromBody] DocumentDto json)
        {
            _studentCmd.SetTherapyReportPath(json.Id, json.Path);
            var resp = new ApiResponse().Create200().WithMessage("Student therapy report url added");
            return Json(resp);
        }

        [HttpPost("psycologicalAssessmentReport")]
        public IActionResult PsycologicalAssessmentReport([FromBody] DocumentDto json)
        {
            _studentCmd.SetPsycologicalAssessmentReportPath(json.Id, json.Path);
            var resp = new ApiResponse().Create200().WithMessage("Student psycological assessment report url added");
            return Json(resp);
        }

        /// <summary>
        /// Get baseline data collection form
        /// </summary>
        /// <returns></returns>
        [HttpGet("baselineDataForm")]
        public IActionResult GetBaselineDataForm()
        {
            var _data = _studentQry.GetBaselineDataCollectionForm();
            var resp = new ApiResponse().Create200().WithData(_data);
            return Json(resp);
        }

        /// <summary>
        /// Created baseline data
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("createBaselineData")]
        public IActionResult CreateBaselineDataForm([FromBody] CreateBaselineDataDto dto)
        {
            try
            {
                _studentCmd.CreateBaselineDataCollectionForm(dto.Name, dto.DOB, dto.AgeAsOnDOOb, dto.DOOb, dto.Language, dto.TherapistName, dto.Gender, dto.BaselinePTOT, dto.BaselineSpeechLanguage, dto.ConsultedPTOT, dto.consultedSpeechLanguage);
                var resp = new ApiResponse().Create200().WithMessage("Baseline data is created");
                return Json(resp);
            }
            catch (Exception ex)
            {
                var errResp = new ApiResponse().WithMessage(ex.Message);
                return Json(errResp);
            }
        }

        /// <summary>
        /// Get baseline data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("baselineData")]
        public IActionResult GetBaselineData([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                var result = _studentQry.GetBaselineDataCollection();
                var resp = new ApiResponse().Create200().WithData(result);
                return Json(resp);
            }
            else
            {
                var result = _studentQry.GetBaselineDataById(id);
                var resp = new ApiResponse().Create200().WithData(result);
                return Json(resp);
            }
        }
        [HttpPost("photo")]
        public IActionResult Photo([FromBody] DocumentDto json)
        {
            _studentCmd.SetPhotoPath(json.Id, json.Path);
            var resp = new ApiResponse().Create200().WithMessage("Student photo url added");
            return Json(resp);
        }
    }
}