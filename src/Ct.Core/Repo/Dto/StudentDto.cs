using System;
using System.Collections.Generic;
using ct._Core.Repo.Resource;

namespace ct._Core.Repo.Dto
{
    /// <summary>
    /// The schema / shape of the class is designed keeping in mind the worker that handles the job. Elixir worker in this case.
    /// </summary>
    public class StudentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string StudentId { get; set; }
        public string Email { get; set; }
        public DateTime DOB { get; set; }
        public DateTime DOA { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string GuardianName { get; set; }
        public string AadharNumber { get; set; }
        public int RollNo { get; set; }
        public GenderEnum Gender { get; set; }
        public StudentStatusEnum Status {get; set;}
        public string SectionName {get;set;}
        public List<string> Tags {get;set;}

    }
     public enum StudentStatusEnum
    {
        Unknown = -1,
        Active = 1,
        Disabled = 99
    }
}
