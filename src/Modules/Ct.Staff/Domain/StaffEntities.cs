namespace Ct.Staff.Domain;

public enum StaffStatus { Unknown = -1, Active = 1, Disabled = 99 }
public enum StaffType { Teaching = 0, NonTeaching = 1, Management = 2 }
public enum EmploymentStatus { Active = 1, Relieved = 90, Disabled = 99 }

public class StaffMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string? StaffCode { get; set; }
    public string Name { get; set; } = "";
    public string? Alias { get; set; }
    public string? Gender { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryMobile { get; set; }
    public string? Designation { get; set; }
    public StaffType StaffType { get; set; } = StaffType.Teaching;
    public StaffStatus Status { get; set; } = StaffStatus.Active;
    public string? RefId { get; set; }
    public List<string> InstituteIds { get; set; } = [];
    public List<string> Subjects { get; set; } = [];
    public List<EmploymentRecord> EmploymentHistory { get; set; } = [];
    public List<DesignationRecord> Designations { get; set; } = [];
    public List<SalaryRecord> Salaries { get; set; } = [];
    public List<TtSlot> Timetable { get; set; } = [];
    public string? ClassTeacherCourseId { get; set; }
    public string? ClassTeacherSection { get; set; }
    public Dictionary<string, string> Meta { get; set; } = new();
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class EmploymentRecord
{
    public string InstId { get; set; } = "";
    public string? Designation { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? RefId { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public List<string> CourseSubjects { get; set; } = [];
}

public class DesignationRecord
{
    public string InstId { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime From { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }
    public StaffType StaffType { get; set; }
}

public class SalaryItem
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
}

public class SalaryRecord
{
    public int Id { get; set; }
    public string InstId { get; set; } = "";
    public List<SalaryItem> Earnings { get; set; } = [];
    public List<SalaryItem> Deductions { get; set; } = [];
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public bool IsPayslip { get; set; }
}

public class TtSlot
{
    public string DayOfWeek { get; set; } = "Monday";
    public int Period { get; set; }
    public string? Subject { get; set; }
    public string? CourseId { get; set; }
    public string? Section { get; set; }
    public string? FromTime { get; set; }
    public string? ToTime { get; set; }
}

public class SalaryStructure
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public Dictionary<string, decimal> Earnings { get; set; } = new();
    public Dictionary<string, decimal> Deductions { get; set; } = new();
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public bool IsCurrent { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
