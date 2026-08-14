using ct._Core.Policy;
using ct._Core.Repo.Dto;
using ct._Core.Repo.Resource;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Newtonsoft.Json;
using Marten;
using Marten.Linq;
using Marten;
using Marten.Linq;
using Repo.Db.Index;
using Repo.Dto;
using Repo.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Marten.Compat;

namespace Repo.Db.Command
{
    /// <summary>
    /// Student Commands
    /// </summary>
    public class StudentCmd
    {
        private readonly IDocumentSession _session;
        private readonly IUserContext _uc;
        private readonly RabbitMQService _rabbitMQService;
        private readonly IPolicyEngine _policyEngine;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="rabbitMQService"></param>
        /// <param name="policyEngineProvider"></param>
        public StudentCmd(IUserContext uc, RabbitMQService rabbitMQService, IPolicyEngineProvider policyEngineProvider)
        {
            _uc = uc.Current();
            _session = _uc.Session;
            _rabbitMQService = rabbitMQService;
            _policyEngine = policyEngineProvider.For(_uc.ShortName);
        }

        /// <summary>
        /// Create student
        /// </summary>
        /// <param name="aadharNo"></param>
        /// <param name="admissionNumber"></param>
        /// <param name="applicationNumber"></param>
        /// <param name="courseName"></param>
        /// <param name="courseId"></param>
        /// <param name="doa"></param>
        /// <param name="dob"></param>
        /// <param name="email"></param>
        /// <param name="gender"></param>
        /// <param name="mobile"></param>
        /// <param name="name"></param>
        /// <param name="secName"></param>
        /// <param name="studentId"></param>
        /// <param name="termName"></param>
        /// <param name="branch"></param>
        /// <param name="uDID"></param>
        /// <param name="fatherOccupationTitle"></param>
        /// <param name="fatherOccupationParticulars"></param>
        /// <param name="fatherOccupationAsOn"></param>
        /// <param name="motherOccupationTitle"></param>
        /// <param name="motherOccupationParticulars"></param>
        /// <param name="motherOccupationAsOn"></param>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="city"></param>
        /// <param name="country"></param>
        /// <param name="state"></param>
        /// <param name="pincode"></param>
        /// <param name="instId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student Create(string aadharNo, string admissionNumber, string applicationNumber, string courseName, string courseId, DateTime doa, DateTime? dob, string email, GenderEnum? gender, string mobile, string name, string secName, string studentId, string termName, string branch, string instId = "", string id = "")
        {
            var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId;
            //ToDo: validate input
            if (_uc.BoundByUser) return null;
            Student student = null;

            if (!string.IsNullOrEmpty(id))
            {
                student = _session.Load<Student>(id);
            }
            if (student == null)
                student = new Student(Person.Create(name, dob, gender));

            // student.Attendance = "Present";
            student.SetId(id);
            student.Enable();
            student.SetPrimaryEMail(email);
            student.SetPrimaryMobile(mobile);
            var next_stu_id = string.IsNullOrEmpty(studentId) ? GetNextStudentId(doa, courseName.Trim()) : studentId;

            var next_admission_no = GetAdmissionNo(admissionNumber, doa);

            student.EnrolInInstitute(inst_id, courseName, courseId, termName, secName, branch, next_stu_id, doa, applicationNumber, next_admission_no);
            student.AadharNumber = aadharNo;
            // student.UDID = udid;

            // var father = new Person { Name = fName, Email = fEmail, Mobile = fMobile };
            // father.SetOccupation(fatherOccupationTitle, fatherOccupationParticulars, fatherOccupationAsOn);
            // father.PAN = fatherPAN;
            // student.Father = father;

            // var mother = new Person { Name = mName, Email = mEmail, Mobile = mMobile };
            // mother.SetOccupation(motherOccupationTitle, motherOccupationParticulars, motherOccupationAsOn);
            // mother.PAN = motherPAN;
            // student.Mother = mother;

            // var address = new Address { Line1 = line1, Line2 = line2, City = city, State = state, Country = country, Pincode = pincode, Status = AddressStatus.Active, Location = LocationType.Home };
            // student.Addresses.Add(address);
            //ToDo: Check? courseCmd.AssignStudents(dto.CourseId, dto.TermName, dto.SecName, new List<string> { student.Id }, false);
            //persist
            if (_uc.Persist)
            {
                _session.Store(student);
                _session.SaveChanges();

                var job_id = Guid.NewGuid().ToString();
                var ids = new List<string>() { student.Id };
                var job = new Job() { id = job_id, title = "student_created", payload = new { student_ids = ids }, client_id = _uc.InstId };
                _rabbitMQService.EnqueStudentCreatedJob(job);

                var fee_worker_job_id = Guid.NewGuid().ToString();
                var job_for_fee_worker = new Job() { id = fee_worker_job_id, title = "student_created", token = _uc.Token };
                _rabbitMQService.EnqueStudentCreatedFeeWorkerJob(job_for_fee_worker);
            }
            return student;
        }

        /// <summary>
        /// Upload Photo
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Student UploadPhoto(string fileName)
        {
            return null;
        }
        internal string AssignStudentId(string id, string studentId)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);
            var stu_id = string.Empty;
            var stu = _session.Load<Student>(id);
            if (stu != null)
            {
                stu_id = string.IsNullOrEmpty(studentId) ? GetNextStudentId(stu.Id) : studentId;
                stu.SetStudentId(inst_id, stu_id);
                _session.Store(stu);
                _session.SaveChanges();
            }
            return stu_id;
        }

        /// <summary>
        /// Enable
        /// </summary>
        /// <param name="id"></param>
        public void Enable(string id)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            if (string.IsNullOrEmpty(id)) return;
            var _student = _session.Load<Student>(id);
            if (_student == null) return;

            var enrol = _student.Enrollments.FirstOrDefault(o => o.InstId == inst_id);
            if (enrol != null) enrol.Enable();

            _session.Store(_student);
            _session.SaveChanges();

            var fw_job_id = Guid.NewGuid().ToString();
            var job_for_fee_worker = new Job() { id = fw_job_id, title = "student_status_updated", token = _uc.Token };
            _rabbitMQService.EnqueFWStudentStatusUpdatedJob(job_for_fee_worker);

            var job_id = Guid.NewGuid().ToString();
            var ids = new List<string>() { _student.Id };
            var job = new Job() { id = job_id, title = "student_updated", payload = new { student_ids = ids }, client_id = _uc.InstId };
            _rabbitMQService.EnqueStudentUpdatedJob(job);
        }

        /// <summary>
        /// Disable
        /// </summary>
        /// <param name="id"></param>
        public void Disable(string id)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            if (string.IsNullOrEmpty(id)) return;
            var _student = _session.Load<Student>(id);
            if (_student == null) return;

            var enrol = _student.Enrollments.FirstOrDefault(o => o.InstId == inst_id);
            if (enrol != null) enrol.Disable();

            _session.Store(_student);
            _session.SaveChanges();

            var fw_job_id = Guid.NewGuid().ToString();
            var job_for_fee_worker = new Job() { id = fw_job_id, title = "student_status_updated", token = _uc.Token };
            _rabbitMQService.EnqueFWStudentStatusUpdatedJob(job_for_fee_worker);

            var job_id = Guid.NewGuid().ToString();
            var ids = new List<string>() { _student.Id };
            var job = new Job() { id = job_id, title = "student_updated", payload = new { student_ids = ids }, client_id = _uc.InstId };
            _rabbitMQService.EnqueStudentUpdatedJob(job);
        }

        /// <summary>
        /// Promote
        /// </summary>
        /// <param name="id"></param>
        /// <param name="baseInstId"></param>
        /// <param name="baseCourseId"></param>
        /// <param name="baseTerm"></param>
        /// <param name="baseSection"></param>
        /// <param name="targetInstId"></param>
        /// <param name="targetCourseId"></param>
        /// <param name="targetTerm"></param>
        /// <param name="targetSection"></param>
        /// <returns></returns>
        public void Promote(string id, string baseInstId, string baseCourseId, string baseTerm, string baseSection, string targetInstId, string targetCourseId, string targetTerm, string targetSection)
        {
            var student = _session.Load<Student>(id);
            if (student == null) throw new ArgumentNullException("Student");

            var baseInstCourses = RedisCache.GetCourseDto(baseInstId);
            var baseCourse = baseInstCourses.Find(c => c.Id.Equals(baseCourseId));

            var targetInstCourses = RedisCache.GetCourseDto(targetInstId);
            var targetCourse = targetInstCourses.Find(c => c.Id.Equals(targetCourseId));

            if (baseCourse == null || targetCourse == null) throw new ArgumentException("Incorrect base/target course");

            if (!baseCourse.InstId.Equals(baseInstId) || !targetCourse.InstId.Equals(targetInstId))
                throw new ArgumentException("Mismatch base/target course and inst");

            student.Promote(baseInstId, baseCourseId, targetInstId, targetCourseId, targetTerm, targetSection, targetCourse.Name);
            _session.Store(student);
            _session.SaveChanges();

            var job_id = Guid.NewGuid().ToString();
            var ids = new List<string>() { student.Id };
            var job = new Job() { id = job_id, title = "student_updated", payload = new { student_ids = ids }, client_id = _uc.InstId };
            _rabbitMQService.EnqueStudentUpdatedJob(job);
        }

        /// <summary>
        /// SetHomeAddress
        /// </summary>
        /// <param name="id"></param>
        /// <param name="city"></param>
        /// <param name="country"></param>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="pinCode"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Student SetHomeAddress(string id, string city, string country, string line1, string line2, string pinCode, string state)
        {
            var result = _session.Load<Student>(id);
            result.SetHomeAddress(line1, line2, city, state, country, pinCode);
            _session.Store(result);
            _session.SaveChanges();
            return result;
        }

        /// <summary>
        /// SetHomePhone
        /// </summary>
        /// <param name="id"></param>
        /// <param name="homePhone"></param>
        /// <returns></returns>
        public Student SetHomePhone(string id, string homePhone)
        {
            var _student = _session.Load<Student>(id);
            _student.SetHomePhone(homePhone);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        ///SetPrimaryEmail
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Student SetPrimaryEmail(string id, string email)
        {
            var _student = _session.Load<Student>(id);
            _student.SetPrimaryEMail(email);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// SetPrimaryMobile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public Student SetPrimaryMobile(string id, string mobile)
        {
            var _student = _session.Load<Student>(id);
            _student.SetPrimaryMobile(mobile);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// Set Tags
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public Student SetTags(string id, List<string> tags)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ArgumentNullException(nameof(_student));
            tags = tags == null ? new List<string>() : tags.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).OrderBy(o => o).ToList();
            _student.Tags = tags;
            _session.Store(_student);
            _session.SaveChanges();
            return _student;
        }

        ///// <summary>
        ///// TC. Reserved for future use.
        ///// </summary>
        ///// <param name="appliedOn"></param>
        ///// <param name="attributes"></param>
        ///// <param name="character"></param>
        ///// <param name="issuedOn"></param>
        ///// <param name="name"></param>
        ///// <param name="remarks"></param>
        ///// <param name="tcNo"></param>
        ///// <returns></returns>
        ////public Student TC(DateTime appliedOn, Dictionary<string, string> attributes, string character, DateTime issuedOn, string name, string remarks, string tcNo)
        ////{
        ////    return null;
        ////}

        internal void TC(string id)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            if (string.IsNullOrEmpty(id)) return;
            var _student = _session.Load<Student>(id);
            if (_student == null) return;

            var enrol = _student.Enrollments.FirstOrDefault(o => o.InstId == inst_id);
            if (enrol != null) enrol.IssueTC();

            _session.Store(_student);
            _session.SaveChanges();

            var fw_job_id = Guid.NewGuid().ToString();
            var job_for_fee_worker = new Job() { id = fw_job_id, title = "student_status_updated", token = _uc.Token };
            _rabbitMQService.EnqueFWStudentStatusUpdatedJob(job_for_fee_worker);

            var job_id = Guid.NewGuid().ToString();
            var ids = new List<string>() { _student.Id };
            var job = new Job() { id = job_id, title = "student_updated", payload = new { student_ids = ids }, client_id = _uc.InstId };
            _rabbitMQService.EnqueStudentUpdatedJob(job);
        }

        /// <summary>
        ///TimeTable
        /// </summary>
        /// <returns></returns>
        public Student TimeTable()
        {
            return null;
        }

        /// <summary>
        ///UpdateFatherDetails
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="mobile"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Student UpdateFatherDetails(string id, string email, string mobile, string name, string pan)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();
            if (_student.Father == null)
            {
                var father = new Person { Name = name, Gender = GenderEnum.Male };
                father.SetId();
                _student.Father = father;
                _session.Store(father);
            }
            else
            {
                _student.Father.ResetName(name);
                _student.Father.Gender = GenderEnum.Male;
            }
            _student.Father.SetPrimaryEMail(email);
            _student.Father.SetPrimaryMobile(mobile);
            _student.Father.PAN = pan;
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        ///UpdateMotherDetails
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="mobile"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Student UpdateMotherDetails(string id, string email, string mobile, string name, string pan)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();
            if (_student.Mother == null)
            {
                var mother = new Person { Name = name, Gender = GenderEnum.Female };
                mother.SetId();
                _student.Mother = mother;
                _session.Store(mother);
            }
            else
            {
                _student.Mother.ResetName(name);
                _student.Mother.Gender = GenderEnum.Female;
            }
            _student.Mother.SetPrimaryEMail(email);
            _student.Mother.SetPrimaryMobile(mobile);
            _student.Mother.PAN = pan;
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// UpdateFatherEmail
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Student UpdateFatherEmail(string id, string email)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null || _student.Father == null) throw new ObjectNotFoundException();
            if (!string.IsNullOrEmpty(email)) _student.Father.SetPrimaryEMail(email);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// UpdateMotherMobile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public Student UpdateMotherMobile(string id, string mobile)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null || _student.Mother == null) throw new ObjectNotFoundException();
            if (!string.IsNullOrEmpty(mobile)) _student.Mother.SetPrimaryMobile(mobile);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// UpdateFatherMobile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public Student UpdateFatherMobile(string id, string mobile)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null || _student.Father == null) throw new ObjectNotFoundException();
            if (!string.IsNullOrEmpty(mobile)) _student.Father.SetPrimaryMobile(mobile);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// UpdateMotherEmail
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Student UpdateMotherEmail(string id, string email)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null || _student.Mother == null) throw new ObjectNotFoundException();
            if (!string.IsNullOrEmpty(email)) _student.Mother.SetPrimaryEMail(email);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// UpdateGuardianDetails
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="gender"></param>
        /// <param name="mobile"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Student UpdateGuardianDetails(string id, string email, GenderEnum gender, string mobile, string name)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();
            if (_student.Guardian == null)
            {
                var guardian = new Person { Name = name, Gender = gender };
                guardian.SetId();
                _student.Guardian = guardian;
                _session.Store(guardian);
            }
            else
            {
                _student.Guardian.ResetName(name);
                _student.Guardian.Gender = gender;
            }
            _student.Guardian.SetPrimaryEMail(email);
            _student.Guardian.SetPrimaryMobile(mobile);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        public SMS SmsAbsentees(string instId = "", DateTime? date = null, int periodNumber = 0, string smsRefId = "")
        {
            var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId;
            var inst_dto = RedisCache.GetClientDto(inst_id);
            var sender_id = inst_dto.SMSSenderId;
            var recipients = GetSMSRecipientDtoForAbsentees(inst_id, date, periodNumber);
            var gateway = _policyEngine.SMSGateway;
            var message = _policyEngine.MessageForAbsentees;
            var sms = SendSMS(gateway, recipients, message, smsRefId, sender_id);
            return sms;
        }

        /// <summary>
        /// UpdateGuardianMobile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public Student UpdateGuardianMobile(string id, string mobile)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null || _student.Guardian == null) throw new ObjectNotFoundException();
            if (!string.IsNullOrEmpty(mobile)) _student.Guardian.SetPrimaryMobile(mobile);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// Update Guardian Email
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Student UpdateGuardianEmail(string id, string email)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null || _student.Guardian == null) throw new ObjectNotFoundException();
            if (!string.IsNullOrEmpty(email)) _student.Guardian.SetPrimaryEMail(email);
            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }
        internal SMS SendSMS(SMSGatewayEnum gateway, IEnumerable<SMSRecipientDto> recipients, string message, string smsRefId, string senderId)
        {
            var inst_id = _uc.InstId;
            var user_id = _uc.UserId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);
            if (recipients == null || string.IsNullOrEmpty(message)) throw new ArgumentNullException();
            List<string> mobiles = recipients.Select(o => o.mobile).Distinct().ToList();
            var sms = new SMS()
            {
                Gateway = gateway,
                Recipients = recipients.ToList(),
                Message = message,
                CreatedBy = user_id,
                CreatedOn = DateTime.UtcNow,
                SMSRefId = smsRefId
            };
            sms.SetId();
            sms.SetOwner(inst_id);
            _session.Store(sms);
            _session.SaveChanges();
            SMSEnqueJob(mobiles, message, senderId, sms.Id);
            return sms;
        }

        internal Email SendEmail(string type, string from, string subject, IEnumerable<string> recipients, string message, List<Attachment> attachments)
        {
            var inst_id = _uc.InstId;
            var user_id = _uc.UserId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);
            if (recipients == null || string.IsNullOrEmpty(message)) throw new ArgumentNullException();
            List<string> emails = recipients.Distinct().ToList();

            var email = new Email()
            {
                Type = type,
                From = from,
                Subject = subject,
                Message = message,
                Recipients = recipients.ToList(),
                CreatedBy = user_id,
                CreatedOn = DateTime.UtcNow,
            };
            if (attachments.Count != 0)
                email.Attachments = attachments;
            email.SetId();
            email.SetOwner(inst_id);
            _session.Store(email);
            _session.SaveChanges();
            var email_obj = new EmailJobDto()
            {
                subject = subject,
                from = from,
                to = emails,
                html = message,
                topic = _uc.Topic,
                inst_id = _uc.InstId,
                inst_short_name = _uc.ShortName
            };
            if (attachments.Count != 0)
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = attachments.First().FilePath,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = "link/s3"
                });
            _rabbitMQService.EnqueEmail(email_obj);
            return email;
        }

        internal void SMSEnqueJob(List<string> mobiles, string message, string senderId = "", string smsId = "")
        {
            if (mobiles == null || mobiles.Count == 0 || string.IsNullOrEmpty(message)) return;

            var redis = RedisCache.Connection.GetDatabase();
            var inst = redis.StringGet(RedisCache.GetKey_Inst(_uc.InstId));
            var instData = JsonConvert.DeserializeObject<ClientDto>(inst);

            if (string.IsNullOrEmpty(senderId))
            {
                senderId = instData.SMSSenderId;
            }
            var payload = new
            {
                sms_id = smsId,
                sender = senderId,
                to = mobiles,
                message = message
            };

            var id = Guid.NewGuid().ToString();
            var userId = _uc.UserId;
            var instId = _uc.InstId;
            var job = new Job() { id = id, title = "sms_send", topic = _uc.Topic, payload = payload, user_id = userId, client_id = instId };
            _rabbitMQService.EnqueSMS(job);
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="aadharNo"></param>
        /// <param name="id"></param>
        /// <param name="admissionNumber"></param>
        /// <param name="applicationNumber"></param>
        /// <param name="doa"></param>
        /// <param name="DOB"></param>
        /// <param name="gender"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Student Update(string aadharNo, string id, string admissionNumber, string applicationNumber, DateTime? doa, DateTime DOB, GenderEnum gender, string name)
        {
            if (!_uc.BoundByInst) throw new NotInScopeException(nameof(id));
            if (id == null) throw new ArgumentNullException(nameof(id));
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ArgumentNullException(nameof(_student));
            _student.DOB = DOB;
            _student.Name = name;
            _student.Gender = gender;
            _student.AadharNumber = aadharNo;
            _student.AssignApplicationNo(_uc.InstId, applicationNumber);
            _student.AssignAdmissionNo(_uc.InstId, admissionNumber);
            var enrol = _student.Enrollments.FirstOrDefault(o => o.InstId == _uc.InstId);
            if (enrol == null) throw new ArgumentNullException(nameof(enrol));
            enrol.DOA = doa.HasValue ? doa.Value : DateTime.MinValue;
            _session.Store(_student);
            _session.SaveChanges();

            //since the student course may have changed, delete the student profile. 
            if (!string.IsNullOrEmpty(_student.VirtualId)) RedisCache.DeleteKeyProfile(_student.VirtualId);

            var job_id = Guid.NewGuid().ToString();
            var ids = new List<string>() { _student.Id };
            var job = new Job() { id = job_id, title = "student_updated", payload = new { student_ids = ids }, client_id = _uc.InstId };
            _rabbitMQService.EnqueStudentUpdatedJob(job);

            return _student;
        }

        /// <summary>
        /// Get FIlters
        /// </summary>
        /// <returns></returns>
        public Student GetFilters()
        {
            return null;
        }

        /// <summary>
        /// Set The roll no for a stu
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rollNo"></param>
        /// <returns></returns>
        public Student SetRollNo(string id, string rollNo)
        {
            var _student = _session.Load<Student>(id);
            for (var i = 0; i < _student.Enrollments.Count; i++)
            {
                if (_student.Enrollments[i].InstId == _uc.InstId)
                {
                    _student.Enrollments[i].RollNo = rollNo;
                }
            }
            _session.Store(_student);
            _session.SaveChanges();
            return _student;
        }

        /// <summary>
        /// Update the course name for a student
        /// </summary>
        /// <param name="id"></param>
        /// <param name="courseName"></param>
        /// <param name="courseId"></param>
        /// <param name="secName"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public Student UpdateCourse(string id, string courseName, string courseId, string secName, string branch = "")
        {
            var _student = _session.Load<Student>(id);
            for (var i = 0; i < _student.Enrollments.Count; i++)
            {
                if (_student.Enrollments[i].InstId == _uc.InstId)
                {
                    _student.Enrollments[i].CourseName = courseName;
                    _student.Enrollments[i].SectionName = secName;
                    _student.Enrollments[i].CourseId = courseId;
                    _student.Enrollments[i].Branch = branch;
                }
            }
            _session.Store(_student);
            _session.SaveChanges();
            //since the student course may have changed, delete the student profile. 
            if (!string.IsNullOrEmpty(_student.VirtualId)) RedisCache.DeleteKeyProfile(_student.VirtualId);

            var job_id = Guid.NewGuid().ToString();
            var ids = new List<string>() { _student.Id };
            var job = new Job() { id = job_id, title = "student_updated", payload = new { student_ids = ids }, client_id = _uc.InstId };
            _rabbitMQService.EnqueStudentUpdatedJob(job);

            return _student;

        }

        /// <summary>
        /// Update Gender of stu
        /// </summary>
        /// <param name="id"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        public Student UpdateGender(string id, GenderEnum gender)
        {
            var _student = _session.Load<Student>(id);
            for (var i = 0; i < _student.Enrollments.Count; i++)
            {
                if (_student.Enrollments[i].InstId == _uc.InstId)
                {
                    _student.Gender = gender;
                }
            }
            _session.Store(_student);
            _session.SaveChanges();

            var job_id = Guid.NewGuid().ToString();
            var ids = new List<string>() { _student.Id };
            var job = new Job() { id = job_id, title = "student_updated", payload = new { student_ids = ids }, client_id = _uc.InstId };
            _rabbitMQService.EnqueStudentUpdatedJob(job);

            return _student;

        }

        internal void ApplyEvents(List<EventLog> events)
        {
            if (!this._uc.BoundByInst || events == null || events.Count == 0) return;
            var att_events = get_dto_events(events);
            foreach (var att_event in att_events)
            {
                _session.Store(att_event);
            }
            _session.SaveChanges();
        }
        List<AttendanceEvent> get_dto_events(List<EventLog> events)
        {
            var dtos = new List<AttendanceEvent>();
            var _allowed_events = new List<string>() { "Student_Attendance_Taken" };
            var _event = "";
            //event data
            var course_id = "";
            var term_name = "";
            var section_name = "";
            DateTime date;
            int period_no;
            var subject_name = "";
            var isOptionalSubject = false;
            var stu_id = "";
            var staff_id = "";
            var attendance = "";
            foreach (var e in events)
            {
                if (!e.EventDataType.Equals("csv")) continue;
                if (!_allowed_events.Contains(e.Event)) continue;
                _event = e.Event;
                //hydrate data
                var arr = e.EventData.Split(",".ToCharArray());
                course_id = arr[0];
                term_name = arr[1];
                section_name = arr[2];
                date = DateTime.Parse(arr[3]);
                period_no = int.Parse(arr[4]);
                subject_name = arr[5];
                isOptionalSubject = arr[9] == "true";
                stu_id = arr[6];
                staff_id = arr[8];
                attendance = arr[7];
                dtos.Add(new AttendanceEvent()
                {
                    InstId = _uc.InstId,
                    CourseId = course_id,
                    TermName = term_name,
                    SectionName = section_name,
                    Date = date,
                    PeriodNo = period_no,
                    SubjectName = subject_name,
                    IsOptionalSubject = isOptionalSubject,
                    StudentId = stu_id,
                    StaffId = staff_id,
                    Attendance = attendance,
                    CreatedBy = _uc.UserId
                });
            }

            return dtos;
        }

        /// <summary>
        /// Get Next Student Id (auto assigned for Inst). yy{course_prefix}{running number}
        /// </summary>
        /// <param name="doa"></param>
        /// <param name="courseName"></param>
        /// <returns></returns>
        public string GetNextStudentId(DateTime doa, string courseName)
        {
            var next_id = "";
            try
            {
                var inst_id = _uc.InstId;
                var client_dto = RedisCache.GetClientDto(inst_id);
                var yy = doa.Equals(DateTime.MinValue) ? client_dto.AcademicYearFrom.Value.ToString("yy") : doa.ToString("yy");
                var course_prefix = _policyEngine.CoursePrefix(courseName);
                var student_id_prefix = $"{yy}{course_prefix}";
                var cur_id = _session.Query<Student_Inst_Index.Result>()
                    .Where(o => o.InstId == inst_id)
                    .Where(o => o.StudentId.StartsWith(student_id_prefix))
                    .OrderByDescending(o => o.StudentId)
                    .Select(o => o.StudentId)
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(cur_id)) next_id = $"{student_id_prefix}001";
                else
                {
                    var cur_id_str = cur_id.Replace(student_id_prefix, string.Empty);
                    int.TryParse(cur_id_str, out int cur_id_int);
                    var padded_id = (++cur_id_int).ToString("D3"); //2=>002
                    next_id = $"{student_id_prefix}{padded_id}";
                }
            }
            catch (NotImplementedException)
            {
                //TBD: log the error but don't do anything for now
            }
            return next_id;
        }

        /// <summary>
        /// Get Next Student Id (auto assigned for Inst). yy{course_prefix}{running number}
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="instId"></param>
        /// <returns></returns>
        internal string GetNextStudentId(string studentId, string instId = "")
        {
            var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId;
            var next_id = "";
            try
            {
                var stu = _session.Load<Student>(studentId);
                var enrl = stu.Enrollments.SingleOrDefault(o => o.InstId == inst_id);
                next_id = GetNextStudentId(enrl.DOA, enrl.CourseName);
            }
            catch (NotImplementedException)
            {
                //TBD: log the error but don't do anything for now
            }
            return next_id;
        }

        internal string GetAdmissionNo(string admissionNo, DateTime doa)
        {
            var admission_no = string.Empty;

            var auto_generate_admission_no = _policyEngine.AutoGenerateAdmissionNo;

            if (auto_generate_admission_no && string.IsNullOrEmpty(admissionNo))
            {
                var next_no = "";
                var inst_id = _uc.InstId;
                var client_dto = RedisCache.GetClientDto(inst_id);
                var prefix = _policyEngine.AdmissionNoPrefix(client_dto.AcademicYearFrom.Value.Year.ToString(), client_dto.AcademicYearTo.Value.Year.ToString());
                var cur_id = _session.Query<Student_Inst_Index.Result>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.AdmissionNo.StartsWith(prefix))
                                     .OrderByDescending(o => o.AdmissionNo)
                                     .Select(o => o.AdmissionNo)
                                     .FirstOrDefault();
                if (string.IsNullOrEmpty(cur_id)) next_no = $"{prefix}0001";
                else
                {
                    var cur_id_str = cur_id.Replace(prefix, string.Empty);
                    int.TryParse(cur_id_str, out int cur_id_int);
                    var padded_id = (++cur_id_int).ToString("D4"); //2=>0002
                    next_no = $"{prefix}{padded_id}";
                }
                admission_no = next_no;
            }
            else admission_no = admissionNo;
            return admission_no;
        }

        internal List<SMSRecipientDto> GetSMSRecipients(RecipientPostData dto)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);
            var result = new List<SMSRecipientDto>();
            //students
            var stu_recipients = dto.Recipients.Student?.Count > 0 ? dto.Recipients.Student : new List<List<string>>();
            if (stu_recipients.Count > 0)
            {
                string s_pred = get_predicate(stu_recipients);
                result.AddRange(GetMobiles(s_pred, studentMobiles: true, includeTags: dto.IncludeTags));
            }
            //Parents
            var p_recipients = dto.Recipients.Parent?.Count > 0 ? dto.Recipients.Parent : new List<List<string>>();
            if (p_recipients.Count > 0)
            {
                var pred = get_predicate(p_recipients);
                result.AddRange(GetMobiles(pred, parentMobiles: true, includeTags: dto.IncludeTags));
            }
            return result;
        }

        internal List<string> GetEmailRecipients(CreateEmailDto dto)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);
            var result = new List<string>();
            //students
            var stu_recipients = dto.Recipients.Student?.Count > 0 ? dto.Recipients.Student : new List<List<string>>();
            if (stu_recipients.Count > 0)
            {
                string s_pred = get_predicate(stu_recipients);
                result.AddRange(GetEmails(s_pred, studentEmails: true));
            }
            //Parents
            var p_recipients = dto.Recipients.Parent?.Count > 0 ? dto.Recipients.Parent : new List<List<string>>();
            if (p_recipients.Count > 0)
            {
                var pred = get_predicate(p_recipients);
                result.AddRange(GetEmails(pred, parentEmails: true));
            }
            return result;
        }

        internal List<StudentContactDto> GetContacts(RecipientsFacet Recipients = null)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<StudentContactDto>();

            //students
            var recipients = Recipients.Student?.Count > 0 ? Recipients.Student : new List<List<string>>();
            string pred = get_predicate(recipients);
            if (!string.IsNullOrEmpty(pred))
                result.AddRange(GetContacts(pred, studentEmails: true));
            //Parents
            recipients = Recipients.Parent?.Count > 0 ? Recipients.Parent : new List<List<string>>();
            pred = get_predicate(recipients);
            if (!string.IsNullOrEmpty(pred))
                result.AddRange(GetContacts(pred, parentEmails: true));
            return result.DistinctBy(s => s.Id).ToList();
        }

        internal List<StudentContactDto> GetContacts(List<string> limitToCourses = null, List<string> limitToStudents = null)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<StudentContactDto>();

            //qry
            var qry = _session.Query<Student_Inst_Index.Result>();

            //filter
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId == inst_id;
            Expression<Func<Student_Inst_Index.Result, bool>> filterByCourses = o => o.CourseId.In(limitToCourses);
            Expression<Func<Student_Inst_Index.Result, bool>> filterByStudents = o => o.Id.In(limitToStudents);

            qry = qry.Where(filterByInst);
            qry = limitToCourses != null && limitToCourses.Count > 0 ? qry.Where(filterByCourses) : qry;
            qry = limitToStudents != null && limitToStudents.Count > 0 ? qry.Where(filterByStudents) : qry;

            using (var enumerator = _session.Stream(qry.OfType<Student>()))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    if (stu == null) continue;
                    var stu_contact_dto = get_contact_dto(stu);
                    result.Add(stu_contact_dto);
                }
            }
            return result;
        }

        internal List<StudentContactDto> GetContacts(string lucenePredicate = "", bool parentEmails = false, bool studentEmails = false)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<StudentContactDto>();

            lucenePredicate = lucenePredicate == "" ? lucenePredicate : $"({lucenePredicate})";
            var predicates = new List<string>() { lucenePredicate, $"(InstId:{inst_id})", $"(EnrollmentStatus:{EnrollmentStatusEnum.Active})" };
            var predicate = string.Join(" AND ", predicates.Where(s => !string.IsNullOrEmpty(s)));
            var qry = _session.Advanced
                            .Query<Student_Inst_Index.Result>()
                            .WhereLucene("Name", predicate)
                            .OfType<Student>();

            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    if (stu != null)
                    {
                        var stu_contact_dto = get_contact_dto(stu);
                        result.Add(stu_contact_dto);
                    }
                }
            }
            return result;
        }

        private StudentContactDto get_contact_dto(Student stu)
        {
            var enrl = stu.Enrollments.First(o => o.InstId == _uc.InstId);
            var attendance_session_count = _policyEngine.AttendanceSessions;

            var dto = new StudentContactDto()
            {
                Id = stu.Id,
                StudentId = enrl.StudentId,
                InstId = enrl.InstId,
                CourseId = enrl.CourseId,
                CourseName = enrl.CourseName,
                Section = enrl.SectionName,
                Status = enrl.Status,
                Emails = stu.GetEmails(),
                Mobiles = stu.GetMobiles(),
            };
            return dto;
        }

        /// <summary>
        /// Get Predicate
        /// </summary>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public string get_predicate(List<List<string>> recipients)
        {
            var studentPredicates = new List<string>();
            var lucenePredicates = new List<string>();
            foreach (var plist in recipients)
            {
                if (plist == null || plist.Count != 4) break;
                if (plist.Last() != "*") //specific student
                {
                    studentPredicates.Add($"Id:{plist[3]}");
                    continue;
                }
                else if (plist.First().Equals("*")) //all courses & hence all students.
                {
                    studentPredicates.Clear();
                    lucenePredicates.Clear();
                    break;
                }
                else
                {
                    var x = plist;
                    var c = x[0];
                    var t = x[1];
                    var s = x[2];

                    var q = $"CourseId:{c}";
                    q = t == "*" ? q : q + $" AND TermName:{t}";
                    q = s == "*" ? q : q + $" AND SectionName:{s}";
                    lucenePredicates.Add($"({q})");
                }
            }
            var stuPred = studentPredicates.Count == 0 ? "" : "(" + string.Join(" OR ", studentPredicates) + ")";
            lucenePredicates.Add(stuPred);
            var pred = string.Join(" OR ", lucenePredicates.Where(s => !string.IsNullOrEmpty(s)));
            return pred;
        }

        internal List<string> GetMobiles(List<string> studentIds)
        {
            var result = new List<string>();
            if (studentIds == null || studentIds.Count == 0) return result;
            var students = _session.Load<Student>(studentIds);
            foreach (var de in students)
            {
                var stu = de.Value;
                if (stu != null)
                {
                    var fn = stu.Father?.Name;
                    var fm = stu.Father?.GetPrimaryMobile();

                    var mn = stu.Mother?.Name;
                    var mm = stu.Mother?.GetPrimaryMobile();

                    var gn = stu.Guardian?.Name;
                    var gm = stu.Guardian?.GetPrimaryMobile();

                    result.Add(stu.GetPrimaryMobile());
                    result.Add(fm);
                    result.Add(mm);
                    result.Add(gm);
                }
            }

            return result
                        .Where(o => !string.IsNullOrEmpty(o))
                        .Distinct()
                        .ToList();
        }

        internal List<SMSRecipientDto> GetSMSRecipientDtoForAbsentees(string instId = "", DateTime? date = null, int periodNumber = 0)
        {
            var absentee_ids = GetAbsenteeIds(instId, date, periodNumber);
            var result = GetSMSRecipientDtoFromIds(absentee_ids);
            return result;
        }
        internal List<SMSRecipientDto> GetSMSRecipientDtoFromIds(List<string> studentIds)
        {
            var lucene_predicate = $"Id:({string.Join(" ", studentIds)})";
            var result = GetMobiles(lucene_predicate, parentMobiles: true, studentMobiles: true);
            return result;
        }

        /// <summary>
        /// Returns the list of Mobile nos of Parent/Guardian of absentees
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="date"></param>
        /// <param name="periodNumber"></param>
        /// <returns></returns>
        public List<string> GetAbsenteeIds(string instId = "", DateTime? date = null, int periodNumber = 0)
        {
            if (instId == "") instId = _uc.InstId;
            var _dt = date.HasValue ? date.Value.Date : DateTime.Now.Date;
            var qry = _session.Query<AttendanceEvent_IX.Result>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where(o => o.Date == _dt)
                            .OfType<AttendanceEvent>();

            var att_events = new List<AttendanceEvent>();
            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var att = enumerator.Current.Document;
                    if (att == null) continue;
                    if (periodNumber != 0 && att.PeriodNo != periodNumber) continue;
                    att_events.Add(att);
                }
            }

            var stu_att = att_events.GroupBy(o => o.StudentId)
                             .Select(g => new { StudentId = g.Key, Att = g.OrderByDescending(s => s.CreatedOn).First()?.Attendance })
                             .ToList();

            var absentee_ids = new List<string>();

            foreach (var sa in stu_att)
            {
                if (sa.Att.Equals("absent", StringComparison.CurrentCultureIgnoreCase)) absentee_ids.Add(sa.StudentId);
            }

            return absentee_ids;
        }



        internal List<SMSRecipientDto> GetMobiles(string lucenePredicate = "", bool parentMobiles = false, bool studentMobiles = false, bool includeTags = false)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<SMSRecipientDto>();

            lucenePredicate = lucenePredicate == "" ? lucenePredicate : $"({lucenePredicate})";
            var predicates = new List<string>() { lucenePredicate, $"(InstId:{inst_id})", $"(EnrollmentStatus:{EnrollmentStatusEnum.Active})" };

            if (includeTags) predicates.Add($"(Tags:(ios oc))");

            var predicate = string.Join(" AND ", predicates.Where(s => !string.IsNullOrEmpty(s)));
            var qry = _session.Advanced
                            .Query<Student_Inst_Index.Result>()
                            .WhereLucene("Name", predicate)
                            .OfType<Student>();

            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    if (stu != null)
                    {
                        var mobile_arr = stu.MobileCSV?.Split(",".ToCharArray());
                        if (mobile_arr == null || mobile_arr.Count() == 0) continue;
                        var sm = mobile_arr.Count() > 0 ? mobile_arr[0] : string.Empty;
                        var fm = mobile_arr.Count() > 1 ? mobile_arr[1] : string.Empty;
                        var mm = mobile_arr.Count() > 2 ? mobile_arr[2] : string.Empty;
                        var gm = mobile_arr.Count() > 3 ? mobile_arr[3] : string.Empty;
                        if (studentMobiles && IsValidMobile(sm)) result.Add(new SMSRecipientDto() { name = stu.Name, mobile = sm });
                        if (parentMobiles && IsValidMobile(fm)) result.Add(new SMSRecipientDto() { name = stu.Father?.Name, mobile = fm });
                        if (parentMobiles && IsValidMobile(mm)) result.Add(new SMSRecipientDto() { name = stu.Mother?.Name, mobile = mm });
                        if (parentMobiles && IsValidMobile(gm)) result.Add(new SMSRecipientDto() { name = stu.Guardian?.Name, mobile = gm });
                    }
                }
            }
            return result;
        }

        internal List<string> GetEmails(string lucenePredicate = "", bool parentEmails = false, bool studentEmails = false)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<string>();

            lucenePredicate = lucenePredicate == "" ? lucenePredicate : $"({lucenePredicate})";
            var predicates = new List<string>() { lucenePredicate, $"(InstId:{inst_id})", $"(EnrollmentStatus:{EnrollmentStatusEnum.Active})" };
            var predicate = string.Join(" AND ", predicates.Where(s => !string.IsNullOrEmpty(s)));
            var qry = _session.Advanced
                            .Query<Student_Inst_Index.Result>()
                            .WhereLucene("Name", predicate)
                            .OfType<Student>();

            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    if (stu != null)
                    {
                        var email_arr = stu.EmailCSV?.Split(",".ToCharArray());
                        if (email_arr == null || email_arr.Count() == 0) continue;
                        var se = email_arr.Count() > 0 ? email_arr[0] : string.Empty;
                        var fe = email_arr.Count() > 1 ? email_arr[1] : string.Empty;
                        var me = email_arr.Count() > 2 ? email_arr[2] : string.Empty;
                        var ge = email_arr.Count() > 3 ? email_arr[3] : string.Empty;
                        if (studentEmails && Helper.IsValidEmail(se)) result.Add(se);
                        if (parentEmails && Helper.IsValidEmail(fe)) result.Add(fe);
                        if (parentEmails && Helper.IsValidEmail(me)) result.Add(me);
                        if (parentEmails && Helper.IsValidEmail(ge)) result.Add(ge);
                    }
                }
            }
            return result;
        }

        internal static bool IsValidMobile(string mobile)
        {
            if (string.IsNullOrEmpty(mobile) || mobile.Trim().Length != 10) return false;
            long number;
            return Int64.TryParse(mobile, out number);
        }

        /// <summary>
        /// Returns the list of Mobile nos of Parent/Guardian of absentees
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="date"></param>
        /// <param name="periodNumber"></param>
        /// <returns></returns>
        public List<string> GetAbsenteeMobiles(string instId = "", DateTime? date = null, int periodNumber = 0)
        {
            if (instId == "") instId = _uc.InstId;
            var _dt = date.HasValue ? date.Value.Date : DateTime.Now.Date;
            var qry = _session.Query<AttendanceEvent_IX.Result>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where(o => o.Date == _dt)
                            .OfType<AttendanceEvent>();

            var att_events = new List<AttendanceEvent>();
            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var att = enumerator.Current.Document;
                    if (att == null) continue;
                    if (periodNumber != 0 && att.PeriodNo != periodNumber) continue;
                    att_events.Add(att);
                }
            }

            var stu_att = att_events.GroupBy(o => o.StudentId)
                             .Select(g => new { StudentId = g.Key, Att = g.OrderByDescending(s => s.CreatedOn).First()?.Attendance })
                             .ToList();

            var absentee_ids = new List<string>();

            foreach (var sa in stu_att)
            {
                if (sa.Att.Equals("absent", StringComparison.CurrentCultureIgnoreCase)) absentee_ids.Add(sa.StudentId);
            }

            var result = GetMobiles(absentee_ids);

            return result;
        }

        /// <summary>
        /// Update domicile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="caste"></param>
        /// <param name="country"></param>
        /// <param name="motherTongue"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Student UpdateDomicile(string id, string caste, string country, string motherTongue, string state)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.Domicile.Caste = caste;
            _student.Domicile.Country = country;
            _student.Domicile.MotherTongue = motherTongue;
            _student.Domicile.State = state;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// Disable father mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student DisableFatherMobile(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var mobile = _student.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();
            if (mobile == null) throw new ObjectNotFoundException();
            mobile.Status = ContactStatus.Disabled;

            _student.Father.Mobile = string.Empty;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// Disable mother mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student DisableMotherMobile(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var mobile = _student.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();
            if (mobile == null) throw new ObjectNotFoundException();
            mobile.Status = ContactStatus.Disabled;

            _student.Mother.Mobile = string.Empty;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// disable guardian mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student DisableGuardianMobile(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var mobile = _student.Guardian.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();
            if (mobile == null) throw new ObjectNotFoundException();
            mobile.Status = ContactStatus.Disabled;

            _student.Guardian.Mobile = string.Empty;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// enable father mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student EnableFatherMobile(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var mobile = _student.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();
            if (mobile == null) throw new ObjectNotFoundException();
            mobile.Status = ContactStatus.Active;

            _student.Father.Mobile = mobile.Info;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// enable mother mobile 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student EnableMotherMobile(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var mobile = _student.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();
            if (mobile == null) throw new ObjectNotFoundException();
            mobile.Status = ContactStatus.Active;

            _student.Mother.Mobile = mobile.Info;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// enable guardian mobile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student EnableGuardianMobile(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var mobile = _student.Guardian.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault();
            if (mobile == null) throw new ObjectNotFoundException();
            mobile.Status = ContactStatus.Active;

            _student.Guardian.Mobile = mobile.Info;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// disable father email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student DisableFatherEmail(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var email = _student.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault();
            if (email == null) throw new ObjectNotFoundException();
            email.Status = ContactStatus.Disabled;

            _student.Father.Email = string.Empty;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// disable mother email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student DisableMotherEmail(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var email = _student.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault();
            if (email == null) throw new ObjectNotFoundException();
            email.Status = ContactStatus.Disabled;

            _student.Mother.Email = string.Empty;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// disable guardian email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student DisableGuardianEmail(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var email = _student.Guardian.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault();
            if (email == null) throw new ObjectNotFoundException();
            email.Status = ContactStatus.Disabled;

            _student.Guardian.Email = string.Empty;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// enable father email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student EnableFatherEmail(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var email = _student.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault();
            if (email == null) throw new ObjectNotFoundException();
            email.Status = ContactStatus.Active;

            _student.Father.Email = email.Info;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// enable mother email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student EnableMotherEmail(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var email = _student.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault();
            if (email == null) throw new ObjectNotFoundException();
            email.Status = ContactStatus.Active;

            _student.Mother.Email = email.Info;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// enable guardian email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Student EnableGuardianEmail(string id)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            var email = _student.Guardian.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault();
            if (email == null) throw new ObjectNotFoundException();
            email.Status = ContactStatus.Active;

            _student.Guardian.Email = email.Info;

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// Add / Update fees receivable
        /// </summary>
        /// <param name="id"></param>
        /// <param name="instId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        // UpdateFeesReceivable, UpdateFeesReceivableStatus, UpdateFeesReceivableForSpanningFee,
        // UpdateFeesReceivableStatusBeforeTx, and UpdateOpeningBalance have been REMOVED.
        // That logic now lives in Fees/Repo/Db/Command/FeesCmd.cs, writing to the Fees
        // module's own database. Student no longer owns or mutates fee data.

        public Student AddIEP(string studentId, List<IEPTerm> terms)
        {
            var inst_id = _uc.InstId;
            var _student = _session.Load<Student>(studentId);
            if (_student == null) throw new ObjectNotFoundException();

            foreach (var t in terms) t.InstId = inst_id;
            _student.IEP.Terms = terms;

            foreach (var t in _student.IEP.Terms)
            {
                foreach (var d in t.Domains) d.Sanitize();
            }

            _session.Store(_student);
            _session.SaveChanges();

            return _student;
        }

        /// <summary>
        /// Add student item
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public StudentEssentials AddStudentEssentials(string studentId, string name, DateTime date, decimal amount)
        {
            var inst_id = _uc.InstId;
            var student_item = new StudentEssentials() { StudentId = studentId };
            student_item.AddItem(name, date, amount, inst_id);

            _session.Store(student_item);
            _session.SaveChanges();

            return student_item;
        }

        public StudentEssentials UpdateItemList(string studentId, string name, DateTime date, decimal amount)
        {
            var inst_id = _uc.InstId;
            var essentials = _session.Query<Student_Essentials_IX.Result>()
                            .Where(o => o.StudentId.Equals(studentId))
                            .OfType<StudentEssentials>()
                            .FirstOrDefault();
            essentials.AddItem(name, date, amount, inst_id);
            _session.Store(essentials);
            _session.SaveChanges();

            return essentials;
        }

        public StudentEssentials UpdateItem(string id, int itemId, string name, DateTime date, decimal amount)
        {
            var inst_id = _uc.InstId;
            var essentials = _session.Load<StudentEssentials>(id);

            if (essentials == null) throw new ObjectNotFoundException();

            var item = essentials.Items.Where(o => o.Id.Equals(itemId)).FirstOrDefault();
            if (item == null) throw new ArgumentNullException();

            item.Name = name;
            item.Date = date;
            item.Amount = amount;

            _session.Store(essentials);
            _session.SaveChanges();

            return essentials;
        }

        public StudentEssentials DisableItem(string id, int itemId)
        {
            var essentials = _session.Load<StudentEssentials>(id);
            if (essentials == null) throw new ObjectNotFoundException();

            var item = essentials.Items.Where(o => o.Id.Equals(itemId)).FirstOrDefault();
            if (item == null) throw new ArgumentNullException();

            item.Status = ItemStatusEnum.Disable;

            _session.Store(essentials);
            _session.SaveChanges();
            return essentials;
        }

        public StudentEssentials EnableItem(string id, int itemId)
        {
            var essentials = _session.Load<StudentEssentials>(id);
            if (essentials == null) throw new ObjectNotFoundException();

            var item = essentials.Items.Where(o => o.Id.Equals(itemId)).FirstOrDefault();
            if (item == null) throw new ArgumentNullException();

            item.Status = ItemStatusEnum.Active;

            _session.Store(essentials);
            _session.SaveChanges();
            return essentials;
        }

        public void SetMedicalDiagnosisReportPath(string id, string documentPath)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.Documents.DiagnosisReport = documentPath;

            _session.Store(_student);
            _session.SaveChanges();
        }

        public void SetDisabilityReportPath(string id, string documentPath)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.Documents.DiagnosisReport = documentPath;

            _session.Store(_student);
            _session.SaveChanges();
        }

        public void SetBirthCertificatePath(string id, string documentPath)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.Documents.BirthCerificate = documentPath;

            _session.Store(_student);
            _session.SaveChanges();
        }

        public void SetIdentificationProofPath(string id, string documentPath)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.Documents.IdentificationProof = documentPath;

            _session.Store(_student);
            _session.SaveChanges();
        }

        public void SetAddressProofPath(string id, string documentPath)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.Documents.AddressProof = documentPath;

            _session.Store(_student);
            _session.SaveChanges();
        }

        public void SetTherapyReportPath(string id, string documentPath)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.Documents.TherapyReport = documentPath;

            _session.Store(_student);
            _session.SaveChanges();
        }

        public void SetPsycologicalAssessmentReportPath(string id, string documentPath)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.Documents.PsycologicalAssessmentReport = documentPath;

            _session.Store(_student);
            _session.SaveChanges();
        }

        public void CreateBaselineDataCollectionForm(string name, DateTime dob, string age, DateTime DOOb, string language, string therapistName, GenderEnum gender, BaselinePTOT pTOT, BaselineSpeechLanguage speechLanguage, bool consultedPTOT = false, bool consultedSpeechLanguage = false)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var baselineData = new BaselineData();
            baselineData.SetId();
            baselineData.SetOwner(inst_id);
            baselineData.Name = name;
            baselineData.DOB = dob;
            baselineData.AgeAsOnDOOb = age;
            baselineData.Gender = gender;
            baselineData.DateOfObservation = DOOb;
            baselineData.TherapistName = therapistName;
            baselineData.Language = language;
            baselineData.BaselinePTOT = pTOT;
            baselineData.BaselineSpeechLanguage = speechLanguage;
            baselineData.ConsultedForPTOT = consultedPTOT;
            baselineData.ConsultedForSpeechLanguage = consultedSpeechLanguage;
            _session.Store(baselineData);
            _session.SaveChanges();
        }
        public void SetPhotoPath(string id, string documentPath)
        {
            var _student = _session.Load<Student>(id);
            if (_student == null) throw new ObjectNotFoundException();

            _student.PhotoUrl = documentPath;

            _session.Store(_student);
            _session.SaveChanges();
        }
    }
}