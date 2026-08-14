using ct._Core.Repo.Resource;
using ct._Core.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repo.Resource
{
    /// <summary>
    /// Student Class
    /// </summary>
    public class Student : Person
    {
        /// <summary>
        /// Enrollments
        /// </summary>
        /// <returns></returns>
        public List<Enrollment> Enrollments { get; set; }

        /// <summary>
        /// StudentId
        /// </summary>
        /// <returns></returns>
        public string StudentId { get; set; }

        /// <summary>
        /// Student Attendance
        /// </summary>
        /// <returns></returns>
        public string Attendance { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        /// <returns></returns>
        public string Category { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        /// <returns></returns>
        public StudentStatusEnum Status { get; set; }

        /// <summary>
        /// Father
        /// </summary>
        /// <returns></returns>

        public Person Father { get; set; }
        /// <summary>
        /// Mother
        /// </summary>
        /// <returns></returns>
        public Person Mother { get; set; }
        /// <summary>
        /// Guardian
        /// </summary>
        /// <returns></returns>
        public Person Guardian { get; set; }
        /// <summary>
        /// Aadhar No
        /// </summary>
        public string AadharNumber { get; set; }
        /// <summary>
        /// UDID (Unique Disability ID)
        /// </summary>
        /// <value></value>
        public string UDID { get; set; }

        /// <summary>
        /// Domicile
        /// </summary>
        /// <value></value>
        public Domicile Domicile { get; set; }
        /// <summary>
        /// User id
        /// </summary>
        /// <value></value>
        public string UserId { get; set; }
        // FeesReceivable was removed from this module. Fee data now lives entirely in the
        // Fees module's own database (Repo/Resource/FeesReceivable.cs there). If this module
        // needs to display fee status, it reads Student/Views/StudentFeeSummary.cs instead.
        public IndividualEducationPlan IEP { get; set; }
        public Documents Documents { get; set; }
        public string PhotoUrl { get; set; }
        /// <summary>
        /// Student Ctor
        /// </summary>
        public Student()
        {
            this.Enable();
            this.Father = new Person();
            this.Mother = new Person();
            this.Guardian = new Person();
            this.Enrollments = new List<Enrollment>();
            this.Contacts = new List<Contact>();
            this.Domicile = new Domicile();
            this.IEP = new IndividualEducationPlan();
            this.Documents = new Documents();
        }

        /// <summary>
        /// Student
        /// </summary>
        /// <param name="p"></param>
        public Student(Person p)
            : this()
        {
            this.SetId();
            this.Name = p.Name;
            this.DOB = p.DOB;
            this.Gender = p.Gender;
            this.Contacts = p.Contacts == null ? new List<Contact>() : p.Contacts;
        }

        /// <summary>
        /// Disable
        /// </summary>
        public void Disable()
        {
            this.Status = StudentStatusEnum.Disabled;
        }

        /// <summary>
        /// Enable
        /// </summary>
        public void Enable()
        {
            this.Status = StudentStatusEnum.Active;
        }

        /// <summary>
        /// EnrolInInstitute
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="courseName"></param>
        /// <param name="courseId"></param>
        /// <param name="termName"></param>
        /// <param name="branch"></param>
        /// <param name="secName"></param>
        /// <param name="studentId"></param>
        /// <param name="applicationNo"></param>
        /// <param name="admissionNo"></param>
        /// <param name="doa"></param>
        /// <returns></returns>
        public Enrollment EnrolInInstitute(string instId, string courseName, string courseId, string termName, string secName, string branch, string studentId, DateTime doa, string applicationNo = "", string admissionNo = "")
        {
            if (string.IsNullOrEmpty(instId)) throw new ArgumentNullException();
            this.Enrollments = this.Enrollments == null ? new List<Enrollment>() : this.Enrollments;
            var enrollment = this.Enrollments.Where(o => o.InstId == instId).SingleOrDefault();
            if (enrollment == null)
            {
                enrollment = new Enrollment() { InstId = instId };
                this.Enrollments.Add(enrollment);
            }
            enrollment.SetCourse(courseName, courseId, termName, secName, branch);
            enrollment.ApplicationNo = applicationNo;
            enrollment.AdmissionNo = admissionNo;
            enrollment.DOA = doa;
            enrollment.StudentId = studentId;
            return enrollment;
        }

        /// <summary>
        /// Set StudentId (assigned by Inst)
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="studentId"></param>
        public void SetStudentId(string instId, string studentId)
        {
            if (Enrollments == null || Enrollments.Count == 0) return;
            var enrl = Enrollments.FirstOrDefault(o => o.InstId == instId);
            if (enrl == null) return;
            enrl.StudentId = studentId;
        }

        /// <summary>
        /// Promote
        /// </summary>
        /// <param name="baseInstId"></param>
        /// <param name="baseCourseId"></param>
        /// <param name="targetInstId"></param>
        /// <param name="targetCourseId"></param>
        /// <param name="targetTerm"></param>
        /// <param name="targetSection"></param>
        public void Promote(string baseInstId, string baseCourseId, string targetInstId, string targetCourseId, string targetTerm = "", string targetSection = "", string targetCourseName = "")
        {
            this.Enrollments = this.Enrollments == null ? new List<Enrollment>() : this.Enrollments;
            var baseEnrollment = this.Enrollments.Where(o => o.InstId.Equals(baseInstId) && o.CourseId.Equals(baseCourseId)).SingleOrDefault();
            if (baseEnrollment == null) return;
            this.Enrollments.RemoveAll(o => o.InstId.Equals(targetInstId));

            var newEnrollment = baseEnrollment.Clone(targetInstId, targetCourseId);
            newEnrollment.TermName = string.IsNullOrEmpty(targetTerm) ? baseEnrollment.TermName : targetTerm;
            newEnrollment.SectionName = string.IsNullOrEmpty(targetSection) ? baseEnrollment.SectionName : targetSection;
            newEnrollment.CourseName = string.IsNullOrEmpty(targetCourseName) ? baseEnrollment.CourseName : targetCourseName;
            this.Enrollments.Add(newEnrollment);
        }

        /// <summary>
        /// AssignInstitute
        /// </summary>
        /// <param name="instId"></param>
        /// <returns></returns>
        public Enrollment AssignInstitute(string instId)
        {
            if (string.IsNullOrEmpty(instId)) throw new ArgumentNullException();
            this.Enrollments = this.Enrollments == null ? new List<Enrollment>() : this.Enrollments;
            var enrol = this.Enrollments.Where(o => o.InstId == instId).SingleOrDefault();
            if (enrol == null)
            {
                enrol = new Enrollment() { InstId = instId };
                this.Enrollments.Add(enrol);
            }
            return enrol;
        }

        /// <summary>
        /// AssignApplicationNo
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="applicationNo"></param>
        public void AssignApplicationNo(string instId, string applicationNo)
        {
            var enrol = this.Enrollments.Where(o => o.InstId == instId).SingleOrDefault();
            if (enrol == null) enrol = this.AssignInstitute(instId);
            enrol.ApplicationNo = applicationNo;
        }

        /// <summary>
        /// AssignAdmissionNo
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="admissionNo"></param>
        public void AssignAdmissionNo(string instId, string admissionNo)
        {
            var enrol = this.Enrollments.Where(o => o.InstId == instId).SingleOrDefault();
            if (enrol == null) enrol = this.AssignInstitute(instId);
            enrol.AdmissionNo = admissionNo;
        }

        /// <summary>
        /// SetCourse
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="courseId"></param>
        /// <param name="termName"></param>
        /// <param name="secName"></param>
        public void SetCourse(string instId, string courseName, string courseId, string termName, string secName, string branch)
        {
            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(instId)) return;
            var enrol = this.Enrollments.Where(o => o.InstId == instId).SingleOrDefault();
            if (enrol == null) enrol = this.AssignInstitute(instId);
            if (enrol != null) enrol.SetCourse(courseName, courseId, termName, secName, branch);
        }
        /// <summary>
        /// TBD: Re-check logic. Order of enrollment may affect the result. 
        /// </summary>
        /// <returns></returns>
        public string GetCurrentCourseId(string instId = "")
        {
            if (this.Enrollments == null || Enrollments.Count == 0) return string.Empty;
            if (string.IsNullOrEmpty(instId))
                return Enrollments.First().CourseId;
            else
            {
                var enrol = Enrollments.FirstOrDefault(o => o.InstId.Equals(instId));
                return enrol == null ? string.Empty : enrol.CourseId;
            }
        }

        /// <summary>
        /// UpdateRollNoInEnrollment
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="courseId"></param>
        /// <param name="rollNo"></param>
        public void UpdateRollNoInEnrollment(string instId, string courseId, string rollNo)
        {
            if (this.Enrollments != null)
            {
                var enr = Enrollments.Where(o => o.InstId.Equals(instId) && o.CourseId.Equals(courseId)).FirstOrDefault();
                if (enr != null)
                    enr.RollNo = rollNo;
            }
        }

        /// <summary>
        /// Update Section in enrollment
        /// </summary>
        /// <param name="instituteId"></param>
        /// <param name="newSectionName"></param>
        public void UpdateSectionInEnrollment(string instituteId, string newSectionName)
        {
            if (this.Enrollments != null)
            {
                var enr = Enrollments.Single(o => o.InstId == instituteId);
                if (enr != null) enr.SectionName = newSectionName;
            }
        }

        /// <summary>
        /// GetRollNo
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public string GetRollNo(string instId, string courseId)
        {
            if (this.Enrollments != null)
            {
                var enr = Enrollments.Where(o => o.InstId.Equals(instId) && o.CourseId.Equals(courseId)).FirstOrDefault();
                return enr == null ? string.Empty : enr.RollNo;
            }
            return string.Empty;
        }

        /// <summary>
        /// IssueTC
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="tcDto"></param>
        public void IssueTC(string instId, TCDto tcDto)
        {
            var enrol = Enrollments.FirstOrDefault(o => o.InstId == instId);
            if (enrol == null) throw new ArgumentException("Student not enrolled for the institute: " + instId);
            enrol.TCNo = tcDto.TCNo;
            enrol.TCAppliedOn = tcDto.AppliedOn.Value;
            enrol.TCIssuedOn = tcDto.IssuedOn.Value;
            enrol.Character = tcDto.Character;
            enrol.Conduct = tcDto.Conduct;
            enrol.Remarks = tcDto.Remarks;
            enrol.Attributes = tcDto.Attributes;
            enrol.Status = EnrollmentStatusEnum.TC;
        }

        /// <summary>
        /// HasValidContact
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public bool HasValidContact
        {
            get
            {
                return this.HasValidEmail && this.HasValidMobile;
            }
        }

        /// <summary>
        /// HasValidParentalContact
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public bool HasValidParentalContact
        {
            get
            {
                var bParent = Father != null && Father.HasValidMobile && Father.HasValidEmail;
                bParent = Mother != null && Mother.HasValidMobile && Mother.HasValidEmail;
                var bGuardian = Guardian != null && Guardian.HasValidMobile && Guardian.HasValidEmail;
                return bParent || bGuardian;
            }
        }

        /// <summary>
        /// GetEmails
        /// </summary>
        /// <param name="includeStudent"></param>
        /// <returns></returns>
        public List<string> GetEmails(bool includeStudent = false)
        {
            var result = new List<string>();

            var se = this.GetPrimaryEmail();
            se = string.IsNullOrEmpty(se) ? string.Empty : se;
            var ie = this.GetStudentInstituteEmail();
            ie = string.IsNullOrEmpty(ie) ? string.Empty : ie;
            var fe = this.Father?.GetPrimaryEmail();
            fe = string.IsNullOrEmpty(fe) ? string.Empty : fe;
            var me = this.Mother?.GetPrimaryEmail();
            me = string.IsNullOrEmpty(me) ? string.Empty : me;
            var ge = this.Guardian?.GetPrimaryEmail();
            ge = string.IsNullOrEmpty(ge) ? string.Empty : ge;

            if (includeStudent) { result.Add(se); result.Add(ie); }
            result.Add(fe);
            result.Add(me);
            result.Add(ge);
            return result;
        }

        /// <summary>
        /// GetMobiles
        /// </summary>
        /// <param name="includeStudent"></param>
        /// <returns></returns>
        public List<string> GetMobiles(bool includeStudent = false)
        {
            var result = new List<string>();
            var sm = this.GetPrimaryMobile();
            sm = string.IsNullOrEmpty(sm) ? string.Empty : sm;
            var fm = this.Father?.GetPrimaryMobile();
            fm = string.IsNullOrEmpty(fm) ? string.Empty : fm;
            var mm = this.Mother?.GetPrimaryMobile();
            mm = string.IsNullOrEmpty(mm) ? string.Empty : mm;
            var gm = this.Guardian?.GetPrimaryMobile();
            gm = string.IsNullOrEmpty(gm) ? string.Empty : gm;

            if (includeStudent) result.Add(sm);
            result.Add(fm);
            result.Add(mm);
            result.Add(gm);
            return result;
        }

        /// <summary>
        /// EmailCSV
        /// </summary>
        public string EmailCSV { get { return string.Join(",", this.GetEmails(includeStudent: true)); } }

        /// <summary>
        /// MobileCSV
        /// </summary>
        public string MobileCSV { get { return string.Join(",", this.GetMobiles(includeStudent: true)); } }
    }

    // public class Occupation
    // {
    //     public string Title { get; set; }
    //     public string Particulars { get; set; }
    //     public DateTime AsOn { get; set; }
    // }

    /// <summary>
    /// Domicile
    /// </summary>
    public class Domicile
    {
        /// <summary>
        /// Caste
        /// </summary>
        /// <value></value>
        public string Caste { get; set; }
        /// <summary>
        /// Country
        /// </summary>
        /// <value></value>
        public string Country { get; set; }
        /// <summary>
        /// Mother tongue
        /// </summary>
        /// <value></value>
        public string MotherTongue { get; set; }
        /// <summary>
        /// State
        /// </summary>
        /// <value></value>
        public string State { get; set; }
        /// <summary>
        /// ctor
        /// </summary>
        public Domicile()
        {
            Country = "India";
        }
    }

    /// <summary>
    /// StudentStatusEnum
    /// </summary>
    public enum StudentStatusEnum
    {
        Unknown = -1,
        Active = 1,
        Disabled = 99
    }


    /// <summary>
    /// Enrollment
    /// </summary>
    [Serializable]
    public class Enrollment
    {

        /// <summary>
        /// InstId
        /// </summary>
        /// <returns></returns>
        public string InstId { get; set; }
        /// <summary>
        /// CourseId
        /// </summary>
        /// <returns></returns>
        public string CourseId { get; set; }
        public string CourseName { get; set; }

        /// <summary>
        /// TermName
        /// </summary>
        /// <returns></returns>
        public string TermName { get; set; }
        /// <summary>
        /// SectionName
        /// </summary>
        /// <returns></returns>
        public string SectionName { get; set; }
        /// <summary>
        /// SectionName
        /// </summary>
        /// <returns></returns>
        public string Branch { get; set; }
        /// <summary>
        /// RollNo
        /// </summary>
        /// <returns></returns>
        public string RollNo { get; set; }
        /// <summary>
        /// RollNoAsInt
        /// </summary>
        /// <returns></returns>
        public int RollNoAsInt
        {
            get
            {
                int res;
                int.TryParse(RollNo, out res);
                return res;
            }
        }
        private string studentId;
        /// <summary>
        /// StudentId assigned by the institute (if any). Not the Entity Id.
        /// If StudentId is Guid then empty string is returned
        /// </summary>
        public string StudentId
        {
            get
            {
                Guid.TryParse(studentId, out Guid guid);
                return guid == Guid.Empty ? studentId : "";
            }
            set
            {
                studentId = value;
            }
        }

        /// <summary>
        /// Date of admission
        /// </summary>
        public DateTime DOA { get; set; }

        /// <summary>
        /// ApplicationNo
        /// </summary>
        /// <returns></returns>
        public string ApplicationNo { get; set; }
        /// <summary>
        /// AdmissionNo
        /// </summary>
        /// <returns></returns>
        public string AdmissionNo { get; set; }
        /// <summary>
        /// OptionalSubjects
        /// </summary>
        /// <returns></returns>
        public List<string> OptionalSubjects { get; set; }

        /// <summary>
        /// Enrollment
        /// </summary>
        public Enrollment()
        {
            this.Status = EnrollmentStatusEnum.Active;
            this.OptionalSubjects = new List<string>();
            Attributes = new Dictionary<string, object>();
            DOA = DateTime.MinValue;
        }

        /// <summary>
        /// SetCourse
        /// </summary>
        /// <param name="newCourseName"></param>
        /// <param name="newCourseId"></param>
        /// <param name="termName"></param>
        /// <param name="secName"></param>
        public void SetCourse(string newCourseName, string newCourseId, string termName, string secName, string branch)
        {
            if (string.IsNullOrEmpty(newCourseId)) throw new ArgumentException(nameof(newCourseId));
            if (string.IsNullOrEmpty(termName)) throw new ArgumentNullException(nameof(termName));
            this.CourseName = newCourseName;
            this.CourseId = newCourseId;
            this.TermName = termName;
            this.SectionName = secName;
            this.Branch = branch;
        }
        /// <summary>
        /// Clone and replace InstId/CourseId with passed in values
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public Enrollment Clone(string instId, string courseId)
        {
            var clone = ObjectHelper.Clone<Enrollment>(this);
            clone.InstId = instId;
            clone.CourseId = courseId;
            return clone;
        }

        internal void Disable()
        {
            Status = EnrollmentStatusEnum.Disabled;
        }

        internal void Enable()
        {
            Status = EnrollmentStatusEnum.Active;
        }

        internal void IssueTC()
        {
            Status = EnrollmentStatusEnum.TC;
            TCIssuedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// TCNo
        /// </summary>
        /// <returns></returns>
        public string TCNo { get; set; }
        /// <summary>
        /// Applied for TC On
        /// </summary>
        public DateTime? TCAppliedOn { get; set; }
        /// <summary>
        /// Issued TC On
        /// </summary>
        public DateTime? TCIssuedOn { get; set; }
        /// <summary>
        /// Character
        /// </summary>
        /// <returns></returns>
        public string Character { get; set; }
        /// <summary>
        /// Conduct
        /// </summary>
        /// <returns></returns>
        public string Conduct { get; set; }
        /// <summary>
        /// Remarks
        /// </summary>
        /// <returns></returns>
        public string Remarks { get; set; }
        /// <summary>
        /// Attributes
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> Attributes { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        /// <returns></returns>
        public EnrollmentStatusEnum Status { get; set; }

    }

    /// <summary>
    /// EnrollmentStatusEnum
    /// </summary>
    public enum EnrollmentStatusEnum
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// Active
        /// </summary>
        Active = 1,
        /// <summary>
        /// Promoted
        /// </summary>
        Promoted = 10,
        /// <summary>
        /// TC
        /// </summary>
        TC = 98,
        /// <summary>
        /// Disabled
        /// </summary>
        Disabled = 99
    }

    // TxStatusEnum moved to the Fees module — it only ever described fee-transaction status.

    /// <summary>
    /// TCDto
    /// </summary>
    public class TCDto
    {
        /// <summary>
        /// TCNo
        /// </summary>
        /// <returns></returns>
        public string TCNo { get; set; }
        /// <summary>
        /// AppliedOn
        /// </summary>
        /// <returns></returns>
        public DateTime? AppliedOn { get; set; }
        /// <summary>
        /// IssuedOn
        /// </summary>
        /// <returns></returns>
        public DateTime? IssuedOn { get; set; }
        /// <summary>
        /// Character
        /// </summary>
        /// <returns></returns>
        public string Character { get; set; }
        /// <summary>
        /// Conduct
        /// </summary>
        /// <returns></returns>
        public string Conduct { get; set; }
        /// <summary>
        /// Remarks
        /// </summary>
        /// <returns></returns>
        public string Remarks { get; set; }
        /// <summary>
        /// Attributes
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> Attributes { get; set; }
        /// <summary>
        /// TCDto
        /// </summary>
        public TCDto()
        {
            Attributes = new Dictionary<string, object>();
        }
    }

    // FeesReceivable and OpeningBalance moved to Fees/Repo/Resource/FeesReceivable.cs —
    // they described fee data, not student data, and now live in the Fees module's own
    // database, keyed by StudentId instead of embedded in the Student document.

    public class Documents
    {
        public string DiagnosisReport { get; set; }
        public string DisabilityCertificate { get; set; }
        public string BirthCerificate { get; set; }
        public string IdentificationProof { get; set; }
        public string AddressProof { get; set; }
        public string TherapyReport { get; set; }
        public string PsycologicalAssessmentReport { get; set; }
    }
}