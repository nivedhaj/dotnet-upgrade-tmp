using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Documents.Indexes;
using Repo.Resource;

namespace Repo.Db.Index
{
    public class Coursewise_Sectionwise_Student_Index : AbstractIndexCreationTask<Course>
    {
        public class Result
        {
            public string InstId { get; set; }
            public string CourseId { get; set; }
            public string CourseName { get; set; }
            public string BranchName { get; set; }
            public CourseStatusEnum Status { get; set; }
            public string SectionName { get; set; }
        }

        public Coursewise_Sectionwise_Student_Index()
        {
            Map =
                courses =>
                    from course in courses
                    // from term in course.Terms
                    // from section in term.Sections
                    select new Result
                    {
                        InstId = course.InstId,
                        CourseId = course.Id,
                        CourseName = course.Name,
                        BranchName = course.Branch,
                        // SectionName = section.Name,
                        Status = course.Status
                    };
        }
    }

    // public class Coursewise_Sectionwise_Student_X : AbstractTransformerCreationTask<Course>
    // {
    //     public class Result
    //     {
    //         public string InstId { get; set; }
    //         public string CourseId { get; set; }
    //         public string CourseName { get; set; }
    //         public string SectionName { get; set; }
    //         public List<string> ExamSubjectOrder { get; set; }

    //         public List<StudentDetails[]> StuChunks { get; set; }
    //         public List<StudentDetails> Students { get; set; }
    //     }

    //     public class StudentDetails
    //     {
    //         public string StudentId { get; set; }
    //         public string StudentName { get; set; }
    //         public string StudentGender { get; set; }
    //         public StudentStatusEnum StudentStatus { get; set; }
    //         public EnrollmentStatusEnum EnrollmentStatus { get; set; }
    //         public string RollNo { get; set; }
    //         public string Email { get; set; }
    //         public string Mobile { get; set; }
    //     }
    //     public Coursewise_Sectionwise_Student_X()
    //     {
    //         TransformResults =
    //             courses =>
    //                 from course in courses
    //                 from term in course.Terms
    //                 from section in term.Sections
    //                 let inst = LoadDocument<Institute>(course.InstId)
    //                 orderby Enumerable.Reverse(inst.CourseOrder).ToList().IndexOf(course.Name) descending, section.Name
    //                 where section.StudentRolls.Count > 0
    //                 select new Result
    //                 {
    //                     InstId = course.InstId,
    //                     CourseId = course.Id,
    //                     CourseName = course.Name + " " + course.Branch,
    //                     SectionName = section.Name,
    //                     ExamSubjectOrder = course.ExamSubjectOrder,
    //                     Students = (from student in section.StudentRolls
    //                                 let stu = LoadDocument<Student>(student.StudentId)
    //                                 let enrol = stu.Enrollments.FirstOrDefault(o => o.InstId == inst.Id && o.CourseId == course.Id)
    //                                 select new StudentDetails
    //                                 {
    //                                     StudentId = student.StudentId,
    //                                     StudentName = stu.Name,
    //                                     StudentGender = stu.Gender.ToString(),
    //                                     StudentStatus = stu.Status,
    //                                     EnrollmentStatus = enrol.Status,
    //                                     RollNo = enrol.RollNo,
    //                                     Email = stu.EmailCSV,
    //                                     Mobile = stu.MobileCSV,

    //                                 }).ToList()
    //                 };
    //     }
    // }


    // public class Coursewise_Sectionwise_Students_X : AbstractTransformerCreationTask<Course>
    // {
    //     public class Result
    //     {
    //         public string InstId { get; set; }
    //         public string CourseId { get; set; }
    //         public CourseStatusEnum Status { get; set; }

    //         public List<SectionDetails> Sections { get; set; }
    //     }

    //     public class SectionDetails
    //     {
    //         public string TermName { get; set; }
    //         public string SectionName { get; set; }
    //         public List<StudentDto> Students { get; set; }
    //     }

    //     public class StudentDto
    //     {
    //         public string StudentId { get; set; }
    //         public string Name { get; set; }
    //     }
    //     public Coursewise_Sectionwise_Students_X()
    //     {
    //         TransformResults =
    //             courses =>
    //                 from course in courses
    //                 select new Result
    //                 {
    //                     InstId = course.InstId,
    //                     CourseId = course.Id,
    //                     Status = course.Status,
    //                     Sections = (from term in course.Terms
    //                                 from section in term.Sections
    //                                 let studentIds = section.StudentRolls.Select(s => s.StudentId).Distinct().ToArray()
    //                                 let students = LoadDocument<Student>(studentIds)
    //                                 select new SectionDetails
    //                                 {
    //                                     TermName = term.Name,
    //                                     SectionName = section.Name,
    //                                     Students = (from stu in students
    //                                                 where stu.Status == StudentStatusEnum.Active && !string.IsNullOrEmpty(stu.Id)
    //                                                 orderby stu.Name
    //                                                 select new StudentDto
    //                                                 {
    //                                                     StudentId = stu.Id,
    //                                                     Name = stu.Name
    //                                                 }).ToList()
    //                                 }).ToList()
    //                 };
    //     }
    // }

    public class Course_Structure_Index : AbstractIndexCreationTask<Course>
    {
        public class Result
        {
            public string Name { get; set; }
            public string Branch { get; set; }
            public string Id { get; set; }
            public string InstId { get; set; }
            public CourseStatusEnum Status { get; set; }
        }
        public Course_Structure_Index()
        {
            Map = courses =>
                    from course in courses
                    select new Result
                    {
                        Name = course.Name,
                        Branch = course.Branch,
                        Id = course.Id,
                        InstId = course.InstId,
                        Status = course.Status
                    };
        }
    }
    // public class Course_Structure_X : AbstractTransformerCreationTask<Course>
    // {
    //     public class Result
    //     {
    //         public string Id { get; set; }
    //         public string Name { get; set; }
    //         public string Affiliation { get; set; }
    //         public string Branch { get; set; }
    //         public string InstId { get; set; }
    //         public string MarksCardTemplate { get; set; }

    //         public CourseStatusEnum Status { get; set; }
    //         public EduLevelEnum EduLevel { get; set; }
    //         public List<TermResult> Terms { get; set; }
    //         public List<string> ExamSubjectOrder { get; set; }
    //     }
    //     public class TermResult
    //     {
    //         public string Name { get; set; }
    //         public DateTime From { get; set; }
    //         public DateTime To { get; set; }
    //         public List<SecResult> Sections { get; set; }
    //     }
    //     public class SecResult
    //     {
    //         public string Name { get; set; }
    //         public List<Subject> Subjects { get; set; }
    //         public List<Subject> OptionalSubjects { get; set; }
    //         public List<StudentResult> StudentRolls { get; set; }
    //         public TimeTable TimeTable { get; set; }
    //         public SecResult()
    //         {
    //             this.Subjects = new List<Subject>();
    //             this.OptionalSubjects = new List<Subject>();
    //             this.StudentRolls = new List<StudentResult>();
    //         }
    //     }
    //     public class StudentResult
    //     {
    //         public string StudentId { get; set; }
    //         public string StudentName { get; set; }
    //         public GenderEnum StudentGender { get; set; }
    //         public DateTime? StudentDOB { get; set; }
    //         public string RollNo { get; set; }
    //         public StudentStatusEnum StudentStatus { get; set; }
    //     }
    //     public Course_Structure_X()
    //     {
    //         TransformResults =
    //             courses =>
    //                 from course in courses
    //                 select new Result
    //                 {
    //                     Id = course.Id,
    //                     Name = course.Name,
    //                     Affiliation = course.Affiliation,
    //                     Branch = course.Branch,
    //                     Status = course.Status,
    //                     InstId = course.InstId,
    //                     EduLevel = course.EduLevel,
    //                     ExamSubjectOrder = course.ExamSubjectOrder,
    //                     MarksCardTemplate = course.MarksCardTemplate,
    //                     Terms = (from term in course.Terms
    //                              select new TermResult
    //                              {
    //                                  Name = term.Name,
    //                                  From = term.From,
    //                                  To = term.To,
    //                                  Sections = (from section in term.Sections
    //                                              select new SecResult
    //                                              {
    //                                                  Name = section.Name,
    //                                                  Subjects = (from subject in section.Subjects
    //                                                              where subject.Optional == false
    //                                                              select subject).ToList(),
    //                                                  OptionalSubjects = (from s in section.Subjects
    //                                                                      where s.Optional == true
    //                                                                      select s).ToList(),
    //                                                  StudentRolls = (from student in section.StudentRolls
    //                                                                  let stu = LoadDocument<Student>(student.StudentId)
    //                                                                  select new StudentResult
    //                                                                  {
    //                                                                      StudentId = student.StudentId,
    //                                                                      RollNo = student.RollNo,
    //                                                                      StudentName = stu.Name,
    //                                                                      StudentDOB = stu.DOB,
    //                                                                      StudentGender = stu.Gender,
    //                                                                      StudentStatus = stu.Status
    //                                                                  }).ToList(),
    //                                                  TimeTable = section.TimeTable
    //                                              }).ToList()
    //                              }).ToList()
    //                 };
    //     }
    // }
}