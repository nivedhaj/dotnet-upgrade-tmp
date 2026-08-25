using ct._Core.Repo.Resource;
using ct._Core.Service;
using ct._Core.Util;
using Repo.Resource;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repo.Resource
{

    /// <summary>
    /// Course Inherits from Entity
    /// </summary>
    [Serializable]
    public class Course : Entity
    {
        public string Name { get; set; }
        public string Branch { get; set; }
        public string NameAndBranch
        {
            get
            {
                var brnch = string.IsNullOrEmpty(Branch) ? string.Empty : Branch.Trim();
                var name = string.IsNullOrEmpty(Name) ? string.Empty : Name.Trim();
                return $"{name} {brnch}";
            }
        }
        public EduLevelEnum EduLevel { get; set; }
        public string EduLevelAsString { get { return EduLevel.ToString(); } }
        public string InstId { get; set; }
        public string Affiliation { get; set; }
        public CourseStatusEnum Status { get; set; }
        public string StatusAsString { get { return Status.ToString(); } }
        public List<Term> Terms { get; set; }
        public List<string> ExamSubjectOrder { get; set; }
        /// <summary>
        /// Default order in which the course(s) have to be sorted. Display / Reports etc.
        /// Defaults to -1 => Not sorted
        /// </summary>
        public int SortIndex { get; set; }
        /// <summary>
        /// Represents the course ordinal with respect to org 
        /// </summary>
        /// <value></value>
        public int Rank { get; set; }
        public Course()
        {
            EduLevel = EduLevelEnum.Unknown;
            Enable();
            Terms = new List<Term>();
            SortIndex = -1;
            ExamSubjectOrder = new List<string>();

        }

        /// <summary>
        /// Returns whether Course is valid or not
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            var result = true;
            result = result && !string.IsNullOrEmpty(Name);
            return result;
        }

        /// <summary>
        /// Disable
        /// </summary>
        public void Disable() => Status = CourseStatusEnum.Disabled;

        /// <summary>
        /// Enable
        /// </summary>
        public void Enable() => Status = CourseStatusEnum.Active;

        public void AtPreNursereyLevel() { EduLevel = EduLevelEnum.PreNursery; }
        public void AtNursereyLevel() { EduLevel = EduLevelEnum.Nursery; }
        public void AtSchoolLevel() { EduLevel = EduLevelEnum.School; }
        public void AtUnderGraduateLevel() { EduLevel = EduLevelEnum.UnderGraduate; }
        public void AtGraduateLevel() { EduLevel = EduLevelEnum.Graduate; }
        public void AtPostGraduateLevel() { EduLevel = EduLevelEnum.PostGraduate; }

        public void UpdateTerm(Term term)
        {
            if (term == null || string.IsNullOrEmpty(term.Name))
                throw new Exception("Term / term name is null");
            var termX = this.Terms.SingleOrDefault(o => o.Name.Equals(term.Name));
            if (termX == null)
                throw new Exception(string.Format("Term {0} not found", term.Name));
            termX.From = term.From;
            termX.To = term.To;
        }

        public void RenameTerm(string name, string newName)
        {
            if (string.IsNullOrEmpty(newName))
                throw new Exception(string.Format("Invalid name {0}", newName));
            var termX = this.Terms.SingleOrDefault(o => o.Name.Equals(name));
            if (termX == null)
                throw new Exception(string.Format("Term {0} not found", name));
            termX.Name = newName;
        }

        public void RemoveTerm(string name)
        {
            var termX = this.Terms.SingleOrDefault(o => o.Name.Equals(name));
            if (termX == null)
                throw new Exception(string.Format("Term {0} not found", name));
            Terms.Remove(termX);
        }

        public void AddSection(string termName, string secName)
        {
            var termX = this.Terms.SingleOrDefault(o => o.Name.Equals(termName));
            if (termX == null)
                throw new Exception(string.Format("Term {0} not found", termName));
            termX.AddNewSection(secName);
        }

        public void RenameSection(string termName, string secName, string newName)
        {
            if (string.IsNullOrEmpty(newName))
                throw new Exception(string.Format("Invalid new name {0}", newName));
            var termX = this.Terms.SingleOrDefault(o => o.Name.Equals(termName));
            if (termX == null)
                throw new Exception(string.Format("Term {0} not found", termName));
            termX.RenameSection(secName, newName);
        }

        public void RemoveSection(string termName, string secName)
        {
            var termX = this.Terms.SingleOrDefault(o => o.Name.Equals(termName));
            if (termX == null)
                throw new Exception(string.Format("Term {0} not found", termName));
            termX.RemoveSection(secName);
        }

        public void AddSubject(string termName, string secName, string subject)
        {
            var termX = this.Terms.SingleOrDefault(o => o.Name.Equals(termName, StringComparison.InvariantCultureIgnoreCase));
            if (termX == null)
                throw new ObjectNotFoundException(string.Format("Term {0} not found", termName));
            var sec = termX.Sections.SingleOrDefault(s => s.Name.Equals(secName, StringComparison.InvariantCultureIgnoreCase));
            if (sec == null)
                throw new ObjectNotFoundException(string.Format("Section {0} not found", secName));
            sec.AddSubject(subject);
        }
        public void RenameSubject(string termName, string secName, string subject, string newSubject)
        {
            if (string.IsNullOrEmpty(newSubject)) return;
            var t = Terms.SingleOrDefault(o => o.Name.Equals(termName, StringComparison.InvariantCultureIgnoreCase));
            var s = t?.Sections?.SingleOrDefault(o => o.Name.Equals(secName, StringComparison.InvariantCultureIgnoreCase));
            s?.RenameSubject(subject, newSubject);
            ExamSubjectOrder = ExamSubjectOrder.Select<string, string>(eso => eso == subject ? newSubject : eso).ToList();
        }
        public void RemoveSubject(string termName, string secName, string subject)
        {
            var termX = this.Terms.SingleOrDefault(o => o.Name.Equals(termName, StringComparison.InvariantCultureIgnoreCase));
            if (termX == null)
                throw new Exception(string.Format("Term {0} not found", termName));
            var sec = termX.Sections.SingleOrDefault(s => s.Name.Equals(secName, StringComparison.InvariantCultureIgnoreCase));
            if (sec == null)
                throw new Exception(string.Format("Section {0} not found", secName));
            sec.RemoveSubject(subject);
        }

        /// <summary>
        /// Clone & reset ids
        /// </summary>
        /// <param name="instId">Institute Id for which the course is to be cloned.null Defaults to InstId of course being cloned</param>
        /// <returns></returns>
        public Course Clone(string instId = "")
        {
            var inst_id = string.IsNullOrEmpty(instId) ? InstId : instId;

            var clone = ObjectHelper.Clone<Course>(this);
            clone.SetId();
            clone.SetOwner(inst_id);
            clone.InstId = inst_id;
            clone.CreatedOn = DateTime.MinValue; //set in AuditEntity
            return clone;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        internal List<string> GetSubjects(string section)
        {
            //TBD Handle multiple terms
            var result = new List<string>();
            foreach (var t in this.Terms ?? new List<Term>())
            {
                foreach (var sec in t.Sections ?? new List<Section>())
                {
                    foreach (var sub in sec.Subjects ?? new List<Subject>())
                    {
                        result.Add(sub.Name);
                    }
                }
            }
            var subjects = result.Distinct().OrderBy(o => o).ToList();
            if (ExamSubjectOrder != null)
                subjects = subjects.OrderByDescending(sub => Enumerable.Reverse(ExamSubjectOrder).ToList().IndexOf(sub)).ToList();

            return subjects;
        }



        /// <summary>
        /// Return the student ids that are enrolled to the course. (All terms/sections).
        /// </summary>
        /// <returns></returns>
        public List<StudentRoll> GetAllStudents(string section = "")
        {
            var result = new List<StudentRoll>();
            foreach (var t in this.Terms ?? new List<Term>())
            {
                foreach (var sec in t.Sections ?? new List<Section>())
                {
                    if (!string.IsNullOrEmpty(section) && !sec.Name.Equals(section, StringComparison.InvariantCultureIgnoreCase)) continue;
                    result.AddRange(sec.StudentRolls);
                }
            }
            //Remove duplicates if any and return
            return result.GroupBy(r => r.StudentId).Select(g => g.First()).ToList();
        }

        /// <summary>
        /// Returns the list of subjects (including optional)  by a student
        /// </summary>
        /// <param name="stuId"></param>
        /// <returns></returns>
        public List<string> GetSubjectsForStudent(string stuId)
        {
            var result = new List<string>();
            foreach (var t in this.Terms ?? new List<Term>())
            {
                foreach (var sec in t.Sections ?? new List<Section>())
                {
                    foreach (var sub in sec.Subjects ?? new List<Subject>())
                    {
                        if (sub.Optional)
                        {
                            var enrol = sub.StudentRolls.FirstOrDefault(o => o.StudentId.Equals(stuId));
                            if (enrol != null) result.Add(sub.Name);
                        }
                        else
                        {
                            var enrol = sec.StudentRolls.FirstOrDefault(o => o.StudentId.Equals(stuId));
                            if (enrol != null) result.Add(sub.Name);
                        }
                    }
                }
            }
            var subjects = result.Distinct().OrderBy(o => o).ToList();
            if (ExamSubjectOrder != null)
                subjects = subjects.OrderByDescending(sub => Enumerable.Reverse(ExamSubjectOrder).ToList().IndexOf(sub)).ToList();

            return subjects;
        }

        public List<string> GetSubjectsForTermSection(string termName, string secName)
        {
            var subjects = new List<string>();
            var term = Terms?.FirstOrDefault(o => o.Name.Equals(termName, StringComparison.CurrentCultureIgnoreCase));
            var section = term?.Sections.FirstOrDefault(o => o.Name.Equals(secName, StringComparison.CurrentCultureIgnoreCase));
            if (section == null) return subjects;
            subjects = section.Subjects.Select(o => o.Name).OrderBy(o => o).ToList();
            if (ExamSubjectOrder != null)
                subjects = subjects.OrderByDescending(sub => Enumerable.Reverse(ExamSubjectOrder).ToList().IndexOf(sub)).ToList();

            return subjects.ToList();
        }

    }
}

[Serializable]
public class Term
{
    public string Name { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<Section> Sections { get; set; }

    public Term()
    {
        InitSections();
    }

    public void InitSections()
    {
        if (this.Sections == null)
            Sections = new List<Section>();
    }

    internal void AddNewSection(string name)
    {
        if (Sections != null && Sections.Any(s => s.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            throw new Exception("Section " + name + " already exists");
        var sec = new Section() { Name = name };
        this.Sections.Add(sec);
    }

    internal void AddSections(List<string> secList)
    {
        InitSections();
        this.Sections.Clear();
        if (secList != null && secList.Count > 0)
        {
            foreach (var sec in secList)
            {
                if (string.IsNullOrEmpty(sec) || string.IsNullOrWhiteSpace(sec)) continue;
                var s = new Section() { Name = sec };
                if (!Sections.Contains(s))
                    Sections.Add(s);
            }
        }
    }

    internal void RemoveSection(string name)
    {
        var sec = Sections.SingleOrDefault(s => s.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (sec == null)
            throw new Exception("Section " + name + " does not exist");

        Sections.Remove(sec);
    }

    internal void RenameSection(string name, string newName)
    {
        if (string.IsNullOrEmpty(newName))
            throw new Exception("Invalid new name");
        var sec = Sections.SingleOrDefault(s => s.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (sec == null)
            throw new Exception("Section " + name + " does not exist");
        sec.Name = newName;
    }
}
[Serializable]
public class Section
{
    public string Name { get; set; }
    public List<Subject> Subjects { get; set; }
    public List<StudentRoll> StudentRolls { get; set; }

    public Section()
    {
        Subjects = new List<Subject>();
        StudentRolls = new List<StudentRoll>();
    }
    internal void AddSubject(string subject)
    {
        if (string.IsNullOrEmpty(subject))
            throw new Exception("Null or empty subject");
        var subj = this.Subjects.SingleOrDefault(o => o.Name.Equals(subject));
        if (subj != null)
            throw new Exception(string.Format("Subject {0} already exists", subj));
        Subjects.Add(new Subject() { Name = subject });
    }
    internal void RenameSubject(string subject, string newSubject)
    {
        var subj = Subjects.SingleOrDefault(o => o.Name.Equals(subject));
        if (subj != null && !string.IsNullOrEmpty(newSubject)) subj.Name = newSubject;
    }
    internal void RemoveSubject(string subject)
    {
        if (string.IsNullOrEmpty(subject))
            throw new Exception("Null or empty subject");
        var subj = this.Subjects.SingleOrDefault(o => o.Name.Equals(subject));
        if (subj == null)
            throw new Exception(string.Format("Subject {0} not found", subj));
        Subjects.Remove(subj);
    }
}
[Serializable]
public class Subject
{
    public string Name;
    public bool Optional { get; set; }
    public List<StudentRoll> StudentRolls { get; set; }
}

[Serializable]
public class StudentRoll
{
    public string StudentId { get; set; }
    public string RollNo { get; set; }
}

/// <summary>
/// Course Status
/// </summary>
public enum CourseStatusEnum
{
    /// <summary>
    /// Status: Unknown
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Status: Active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Status: Disabled
    /// </summary>
    Disabled = 99
};

/// <summary>
/// Education Level
/// </summary>
public enum EduLevelEnum
{
    /// <summary>
    /// Unknown / No set / Undefined
    /// </summary>
    Unknown = -1,
    /// <summary>
    /// Pre Nursery / Playhome
    /// </summary>
    PreNursery = 2,
    /// <summary>
    /// Nursery / LKG, UKG
    /// </summary>
    Nursery = 5,
    /// <summary>
    /// School 1-X
    /// </summary>
    School = 10,
    /// <summary>
    /// Under Graduate
    /// </summary>
    UnderGraduate = 20,
    /// <summary>
    /// Graduate
    /// </summary>
    Graduate = 30,
    /// <summary>
    /// Post Graduate
    /// </summary>
    PostGraduate = 40
}

