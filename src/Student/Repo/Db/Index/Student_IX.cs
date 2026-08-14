// Migrated from RavenDB index → Postgres/Marten.
// The nested Result types are kept for query projections.
// Raven Map/Reduce definitions are no longer executed; use Marten LINQ
// against documents or configure property indexes in StudentModule CreateStore.
using ct._Core.Repo.Resource;
using Marten.Schema;
using Repo.Resource;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repo.Db.Index
{
    internal class Student_Inst_Index 
    {
        public class Result
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public GenderEnum? Gender { get; set; }
            public string GenderAsString { get { return Gender.ToString(); } }

            public StudentStatusEnum Status { get; set; }
            public string StatusAsString { get { return Status.ToString(); } }

            public string InstId { get; set; }
            public string StudentId { get; set; }
            public string CourseId { get; set; }
            public string CourseName { get; set; }
            public string Branch { get; set; }
            public string CourseNameAndBranch { get { return $"{CourseName} {Branch ?? string.Empty}".Trim(); } }
            public int CourseSortIndex { get; set; }
            public string TermName { get; set; }
            public string SectionName { get; set; }
            public EnrollmentStatusEnum EnrollmentStatus { get; set; }
            public string EnrollmentStatusAsString { get { return EnrollmentStatus.ToString(); } }
            public int RollNo { get; set; }
            public DateTime? DOB { get; set; }
            public DateTime DOA { get; set; }

            public List<string> Tags { get; set; }
            /// <summary>
            /// Comma separated emails of stu,father,mother,guardian
            /// </summary>
            public string Emails { get; set; }
            public List<string> EmailList { get; set; }
            /// <summary>
            /// Comma separated mobiles of stu,father,mother,guardian
            /// </summary>
            public string Mobiles { get; set; }

            public string FatherName { get; set; }
            public string MotherName { get; set; }
            public string FatherPAN { get; set; }
            public string MotherPAN { get; set; }
            public string VirtualId { get; set; }
            public string OrgId { get; set; }
            public string UserId { get; set; }
            public string AdmissionNo { get; set; }
            // FeesReceivable projection removed — this index lives in the Student module's
            // own database and must not reference Fees-owned data. Fee status is available
            // via Student/Views/StudentFeeSummary.cs instead, populated by a Fees-owned ETL.

            public Result()
            {
                Tags = new List<string>();
            }
        }

        public Student_Inst_Index()
        {
            // RavenDB 7.2: Recurse is not supported, use SelectMany for enrollments
            // LoadDocument is only allowed in LINQ, not in C# methods
            Map = students =>
                from stu in students
                from enrl in stu.Enrollments
                let course = LoadDocument<Course>(enrl.CourseId)
                let institute = LoadDocument<Institute>(enrl.InstId)
                select new
                {
                    Id = stu.Id,
                    Name = stu.Name,
                    Gender = stu.Gender,
                    Status = stu.Status,
                    EnrollmentStatus = enrl.Status,
                    InstId = enrl.InstId,
                    StudentId = enrl.StudentId,
                    CourseId = enrl.CourseId,
                    CourseName = course != null ? course.Name : null,
                    Branch = course != null ? course.Branch : null,
                    CourseSortIndex = course != null ? course.SortIndex : 0,
                    TermName = enrl.TermName,
                    SectionName = enrl.SectionName,
                    RollNo = enrl.RollNoAsInt,
                    Tags = stu.Tags,
                    Emails = stu.EmailCSV,
                    Mobiles = stu.MobileCSV,
                    FatherName = stu.Father != null ? stu.Father.Name : null,
                    MotherName = stu.Mother != null ? stu.Mother.Name : null,
                    FatherPAN = stu.Father != null ? stu.Father.PAN : null,
                    MotherPAN = stu.Mother != null ? stu.Mother.PAN : null,
                    EmailList = stu.EmailCSV != null ? stu.EmailCSV.Split(new[]{","}, StringSplitOptions.None) : null,
                    VirtualId = stu.VirtualId,
                    OrgId = institute != null ? institute.OwnerId : null,
                    DOA = enrl.DOA,
                    DOB = stu.DOB,
                    UserId = stu.UserId,
                    AdmissionNo = enrl.AdmissionNo
                };
            StoreAllFields(FieldStorage.Yes);
            // Enable full-text search on Name field
            Indexes.Add(x => x.Name, FieldIndexing.Search);
            Indexes.Add(x => x.FatherName, FieldIndexing.Search);
            Indexes.Add(x => x.MotherName, FieldIndexing.Search);
        }
    }

    internal class Institute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
    }

    internal class Student_Search 
    {
        public class ReduceResult
        {
            public string Query { get; set; }
            public StudentStatusEnum Status { get; set; }
        }
        public Student_Search()
        {
            Map = students => from stu in students
                              select new
                              {
                                  Query = new object[]
                                  {
                                      stu.Father.Contacts.FirstOrDefault(c => c.ContactType == ContactTypeEnum.Email).Info,
                                      stu.Father.Contacts.FirstOrDefault(c => c.ContactType == ContactTypeEnum.Mobile).Info,
                                      stu.Mother.Contacts.FirstOrDefault(c => c.ContactType == ContactTypeEnum.Email).Info,
                                      stu.Mother.Contacts.FirstOrDefault(c => c.ContactType == ContactTypeEnum.Mobile).Info,
                                      stu.Guardian.Contacts.FirstOrDefault(c => c.ContactType == ContactTypeEnum.Email).Info,
                                      stu.Guardian.Contacts.FirstOrDefault(c => c.ContactType == ContactTypeEnum.Mobile).Info
                                  },
                                  Status = stu.Status
                              };
        }
    }

    internal class Student_Coursewise_Count 
    {
        public class Result
        {
            public string InstId { get; set; }
            public string CourseId { get; set; }
            public string CourseName { get; set; }
            public string TermName { get; set; }
            public string SectionName { get; set; }
            public int Boys { get; set; }
            public int Girls { get; set; }
            public int Total { get { return Boys + Girls; } }
            //public int CourseSortIndex { get; set; }
        }
        public Student_Coursewise_Count()
        {
            Map =
               students =>
                   from stu in students
                   where stu.Status == StudentStatusEnum.Active
                   from enrl in Recurse(stu, x => x.Enrollments)
                   where enrl.Status == EnrollmentStatusEnum.Active
                   let course = LoadDocument<Course>(enrl.CourseId)
                   select new
                   {
                       InstId = enrl.InstId,
                       CourseId = course.Id,
                       CourseName = course.Name,
                       TermName = enrl.TermName,
                       SectionName = enrl.SectionName,
                       Boys = stu.Gender == GenderEnum.Male ? 1 : 0,
                       Girls = stu.Gender == GenderEnum.Female ? 1 : 0
                   };
            Reduce =
                results =>
                from r in results
                group r by new { r.InstId, r.CourseId, r.CourseName, r.TermName, r.SectionName } into grp
                select new Result
                {
                    InstId = grp.Key.InstId,
                    CourseId = grp.Key.CourseId,
                    CourseName = grp.Key.CourseName,
                    TermName = grp.Key.TermName,
                    SectionName = grp.Key.SectionName,
                    Boys = grp.Sum(x => x.Boys),
                    Girls = grp.Sum(x => x.Girls)
                };
        }
    }

    internal class Inst_Student_Count 
    {
        public class Result
        {
            public string InstId { get; set; }
            public int StudentCountMale { get; set; }
            public int StudentCountFemale { get; set; }
            public int TotalStudentCount { get { return StudentCountMale + StudentCountFemale; } }
        }
        public Inst_Student_Count()
        {
            Map =
               students =>
                   from stu in students
                   where stu.Status == StudentStatusEnum.Active
                   from enrl in Recurse(stu, x => x.Enrollments)
                   where enrl.Status == EnrollmentStatusEnum.Active
                   select new
                   {
                       InstId = enrl.InstId,
                       StudentCountMale = stu.Gender == GenderEnum.Male ? 1 : 0,
                       StudentCountFemale = stu.Gender == GenderEnum.Female ? 1 : 0
                   };
            Reduce =
                    results =>
                    from r in results
                    group r by new { r.InstId } into grp
                    select new Result
                    {
                        InstId = grp.Key.InstId,
                        StudentCountMale = grp.Sum(x => x.StudentCountMale),
                        StudentCountFemale = grp.Sum(x => x.StudentCountFemale)
                    };
            StoreAllFields(FieldStorage.Yes);
        }
    }
}