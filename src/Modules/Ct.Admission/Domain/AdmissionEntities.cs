namespace Ct.Admission.Domain;

public enum ApplicationStatus
{
    Wip = 10, Selected = 15, Submitted = 20, Shortlisted = 25,
    Declined = 30, Rejected = 40, Admitted = 50
}
public enum TemplateStatus { Draft = 1, Published = 10, Disabled = 99 }

public class AdmissionApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string? ApplicationNo { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string Name { get; set; } = "";
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Wip;
    public string? Category { get; set; }
    public Dictionary<string, string> Identity { get; set; } = new();
    public Dictionary<string, string> Father { get; set; } = new();
    public Dictionary<string, string> Mother { get; set; } = new();
    public Dictionary<string, string> Guardian { get; set; } = new();
    public List<Dictionary<string, string>> AcademicHistory { get; set; } = [];
    public Dictionary<string, string> Documents { get; set; } = new(); // photo, aadhar, hsc, ssc, caste, etc.
    public Guid? TemplateId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedOn { get; set; }
    public DateTime? AdmittedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class SeatMatrix
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string CourseId { get; set; } = "";
    public string? CourseName { get; set; }
    public int TotalSeats { get; set; }
    public int ReservedSeats { get; set; }
    public int AdmittedCount { get; set; }
    public Dictionary<string, int> CategoryQuota { get; set; } = new();
    public DateTime? ModifiedOn { get; set; }
}

public class ApplicationFormTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public List<string> Fields { get; set; } = [];
    public List<string> ShortlistCriteria { get; set; } = [];
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedOn { get; set; }
}

public class OtpRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MobileOrEmail { get; set; } = "";
    public string Code { get; set; } = "";
    public DateTime ExpiresOn { get; set; }
    public bool Consumed { get; set; }
}
