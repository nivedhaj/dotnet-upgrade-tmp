using ct._Core.Repo.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Repo.Resource;
using System.Linq;

namespace Repo.Dto
{

    /// <summary>
    /// Create student Dto - With annotations
    /// </summary>
    public class CreateStudentDto
    {
        /// <summary>
        /// Aadhar Number
        /// </summary>
        public string AadharNumber { get; set; }
        /// <summary>
        /// AdmissionNumber
        /// </summary>
        /// <returns></returns>
        public string AdmissionNumber { get; set; }

        /// <summary>
        /// ApplicationNumber
        /// </summary>
        /// <returns></returns>
        public string ApplicationNumber { get; set; }

        /// <summary>
        /// CourseId
        /// </summary>
        /// <returns></returns>
        public string CourseId { get; set; }
        /// <summary>
        /// Course Name
        /// </summary>
        [Required]
        public string CourseName { get; set; }

        /// <summary>
        /// DOA
        /// </summary>
        /// <returns></returns>
        public DateTime DOA { get; set; }

        /// <summary>
        /// DOB
        /// </summary>
        /// <returns></returns>
        [Required]
        public DateTime? DOB { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        /// <returns></returns>
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        /// <returns></returns>
        [Required]
        public GenderEnum? Gender { get; set; }

        /// <summary>
        /// Mobile
        /// </summary>
        /// <returns></returns>
        public string Mobile { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        /// <returns></returns>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// SecName
        /// </summary>
        /// <returns></returns>
        [Required]
        public string SecName { get; set; }

        /// <summary>
        /// TermName
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Student Id
        /// </summary>
        /// <returns></returns>
        public string StudentId { get; set; }
        /// <summary>
        /// Branch
        /// </summary>
        public string Branch { get; set; }
       // public string UDID { get; set; }
        /// <summary>
        /// Term Name
        /// </summary>
        /// <returns></returns>
        [Required]
        public string TermName { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// InstId
        /// </summary>
        /// <value></value>
        public string InstId { get; set; }

        // public string FatherName { get; set; }
        // public string FatherEmail { get; set; }
        // public string FatherMobile { get; set; }

        // public string MotherName { get; set; }
        // public string MotherEmail { get; set; }
        // public string MotherMobile { get; set; }

        // public string FatherPAN { get; set; }
        // public string MotherPAN { get; set; }

        // public string Line1 { get; set; }
        // public string Line2 { get; set; }
        // public string City { get; set; }
        // public string Country { get; set; }
        // public string State { get; set; }
        // public string Pincode { get; set; }

        // public string FatherOccupationTitle { get; set; }
        // public string FatherOccupationParticulars { get; set; }
        // public DateTime? FatherOccupationAsOn { get; set; }

        // public string MotherOccupationTitle { get; set; }
        // public string MotherOccupationParticulars { get; set; }
        // public DateTime? MotherOccupationAsOn { get; set; }

    }

    /// <summary>
    /// UpdateGuardianDetailsStudentDto
    /// </summary>
    public class UpdateGuardianDetailsStudentDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        /// <summary>
        ///Name
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        /// <returns></returns>
        public string Email { get; set; }
        /// <summary>
        /// Mobile
        /// </summary>
        /// <returns></returns>
        public string Mobile { get; set; }
        /// <summary>
        /// Gender
        /// </summary>
        /// <returns></returns>
        public GenderEnum Gender { get; set; }

    }

    /// <summary>
    /// SetHomeAddressStudentDto
    /// </summary>
    public class SetHomeAddressStudentDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// Line1
        /// </summary>
        /// <returns></returns>
        public string Line1 { get; set; }
        /// <summary>
        /// Line2
        /// </summary>
        /// <returns></returns>
        public string Line2 { get; set; }
        /// <summary>
        /// City
        /// </summary>
        /// <returns></returns>
        public string City { get; set; }
        /// <summary>
        /// State
        /// </summary>
        /// <returns></returns>
        public string State { get; set; }
        /// <summary>
        /// Pincode
        /// </summary>
        /// <returns></returns>
        public string Pincode { get; set; }
        /// <summary>
        /// Country
        /// </summary>
        /// <returns></returns>
        public string Country { get; set; }
    }

    /// <summary>
    /// TcStudentDto
    /// </summary>
    public class TcStudentDto
    {
        /// <summary>
        /// AppliedOn
        /// </summary>
        /// <returns></returns>
        public DateTime AppliedOn { get; set; }
        /// <summary>
        /// Character
        /// </summary>
        /// <returns></returns>
        public string Character { get; set; }
        /// <summary>
        /// IssuedOn
        /// </summary>
        /// <returns></returns>
        public DateTime IssuedOn { get; set; }
        /// <summary>
        /// naNameme
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; }
        /// <summary>
        /// Remarks
        /// </summary>
        /// <returns></returns>
        public string Remarks { get; set; }
        /// <summary>
        /// TCNo
        /// </summary>
        /// <returns></returns>
        public string TCNo { get; set; }
        /// <summary>
        /// Attributes
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> Attributes { get; set; }
    }

    /// <summary>
    /// TimeTableDto
    /// </summary>
    public class TimeTableDto
    {

    }

    /// <summary>
    /// UpdateFatherDetailsStudentDto
    /// </summary>
    public class UpdateFatherDetailsStudentDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        /// <returns></returns>
        public string Email { get; set; }
        /// <summary>
        /// Mobile
        /// </summary>
        /// <returns></returns>
        public string Mobile { get; set; }
        public string PAN { get; set; }
    }

    /// <summary>
    /// UpdateStudentDto
    /// </summary>
    public class UpdateStudentDto
    {
        /// <summary>
        /// AadharNumber
        /// </summary>
        /// <value></value>
        public string AadharNumber { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; }
        /// <summary>
        /// Gender
        /// </summary>
        /// <returns></returns>
        public GenderEnum Gender { get; set; }
        /// <summary>
        /// AdmissionNo
        /// </summary>
        /// <returns></returns>
        public string AdmissionNo { get; set; }
        /// <summary>
        /// ApplicationNo
        /// </summary>
        /// <returns></returns>
        public string ApplicationNo { get; set; }
        /// <summary>
        /// DOB
        /// </summary>
        /// <returns></returns>
        public DateTime DOB { get; set; }
        /// <summary>
        /// DateOfAdmission
        /// </summary>
        /// <returns></returns>
        public DateTime? DOA { get; set; }
    }

    /// <summary>
    /// UpdateMotherDetailsStudentDto
    /// </summary>
    public class UpdateMotherDetailsStudentDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        /// <returns></returns>
        public string Email { get; set; }
        /// <summary>
        /// Mobile
        /// </summary>
        /// <returns></returns>
        public string Mobile { get; set; }
        public string PAN { get; set; }

    }

    /// <summary>
    /// GetFiltersStudentDto
    /// </summary>
    public class GetFiltersStudentDto
    {

    }

    /// <summary>
    /// StudentPromotionDto
    /// </summary>
    public class StudentPromotionDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <returns></returns>
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        /// <returns></returns>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// CourseId
        /// </summary>
        /// <returns></returns>
        public string CourseId { get; set; }
        /// <summary>
        /// TermName
        /// </summary>
        /// <returns></returns>
        [Required]
        public string TermName { get; set; }
        /// <summary>
        /// SecName
        /// </summary>
        /// <returns></returns>
        [Required]
        public string SecName { get; set; }
    }
    /// <summary>
    /// Home Phone Dto
    /// </summary>
    public class HomePhoneDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// Home Phone
        /// </summary>
        /// <value></value>
        public string HomePhone { get; set; }
    }

    public class SetPrimaryEmailDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
    }

    public class SetPrimaryMobileDto
    {
        public string Id { get; set; }
        public string Mobile { get; set; }
    }

    public class SetRollNoDto
    {
        public string Id { get; set; }
        public string RollNo { get; set; }
    }

    public class UpdateTagsDto
    {
        public string Id { get; set; }
        public List<string> Tags { get; set; }
    }

    public class UpdateCourseDto
    {
        public string Id { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public string CourseId { get; set; }
        public string Branch { get; set; }
    }

    public class UpdateGenderDto
    {
        public string Id { get; set; }
        public GenderEnum Gender { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public string Id { get; set; }
        public string Attendance { get; set; }
    }

    internal class StudentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public DateTime DOB { get; set; }
        public GenderEnum Gender { get; set; }
        public StudentStatusEnum Status { get; set; }
        public string StatusAsString { get { return Status.ToString(); } }

        public string Email { get; set; }


        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string StudentId { get; set; }

        public DateTime DOA { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string GuardianName { get; set; }
        public string AadharNumber { get; set; }
        public int RollNo { get; set; }


        public string SectionName { get; set; }
        public List<string> Tags { get; set; }

    }

    internal class GetStudentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public DateTime? DOB { get; set; }
        public string Gender { get; set; }

        public string Email { get; set; }
        public string Mobile { get; set; }
        public List<string> ParentEmails { get; set; }
        public List<string> ParentMobiles { get; set; }

        public Person Father { get; set; }
        public Person Mother { get; set; }
        public Person Guardian { get; set; }
        public Address Address { get; set; }
        public string PhotoURL { get; set; }

        //enrollment
        public string InstId { get; set; }
        public string StudentId { get; set; }
        public EnrollmentStatusEnum Status { get; set; }
        public string StatusAsString { get { return Status.ToString(); } }
        public string CourseId { get; set; }
        public int CourseSortIndex { get; set; }
        public string CourseName { get; set; }
        public string Branch { get; set; }
        public string TermName { get; set; }
        public string SectionName { get; set; }
        public int RollNo { get; set; }

        public DateTime? DOA { get; set; }
        public string AdmissionNo { get; set; }
        public string ApplicationNo { get; set; }
        public string AadharNumber { get; set; }

        public string MarksCardTemplate { get; set; }
        public string Attendance { get { return AttendanceList.Any(o => o == "Absent") ? "Absent" : "Present"; } }
        public List<string> AttendanceList { get; set; }
        public List<string> Tags { get; set; }
        public string VirtualId { get; set; }
        public Domicile Domicile { get; set; }
        public IndividualEducationPlan IEP { get; set; }
        public GetStudentDto()
        {
            AttendanceList = new List<string>();
            Tags = new List<string>();
            Domicile = new Domicile();
            IEP = new IndividualEducationPlan();
        }
    }

    /// <summary>
    /// Id - Assigned by System
    /// StudentId - Assigned by Institute
    /// </summary>
    public class AssignStudentIdDto
    {
        /// <summary>
        /// Id - Assigned by System
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// StudentId - Assigned by Institute
        /// </summary>
        public string StudentId { get; set; }
    }


    public class HallTicketDto
    {
        public string ContentType { get; set; } //HallTicket | Instructions
        public List<List<string>> Students { get; set; }
    }

    public class CertificateDto : HallTicketDto { }

    internal class StudentProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string SectionName { get; set; }
        public string Branch { get; set; }
    }

    /// <summary>
    /// Update domicile dto
    /// </summary>
    public class UpdateDomicileDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
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
    }

    public class StudentInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
    }
}