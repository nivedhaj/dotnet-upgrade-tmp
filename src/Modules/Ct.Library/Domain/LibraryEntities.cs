namespace Ct.Library.Domain;

public enum MaterialStatus
{
    Available = 1,
    Issued = 2,
    Maintenance = 3,
    Lost = 4,
    Reference = 5,
    Disabled = 99
}

public enum MemberStatus { Active = 1, Disabled = 99 }
public enum CirculationStatus { Issued = 1, Returned = 2, Overdue = 3, Lost = 4 }

public class Material
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string TrackingId { get; set; } = "";
    public string? ISBN { get; set; }
    public string Title { get; set; } = "";
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public string? Location { get; set; }
    public decimal Value { get; set; }
    public int Pages { get; set; }
    public MaterialStatus Status { get; set; } = MaterialStatus.Available;
    public Dictionary<string, string> Attributes { get; set; } = new();
    public List<string> Tags { get; set; } = [];
    public DateTime? LastVerifiedOn { get; set; }
    public DateTime? TxDate { get; set; }
    public DateTime? DueOn { get; set; }
    public Guid? CurrentMemberId { get; set; }
    public string? CoverUrl { get; set; }
    public string? Synopsis { get; set; }
    public DateTime? AcquiredOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class Member
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? MemberCode { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? MemberType { get; set; } // Student, Staff
    public string? RefId { get; set; }
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public Dictionary<string, string> Attributes { get; set; } = new();
    public List<string> Tags { get; set; } = [];
    public int MaxIssues { get; set; } = 3;
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class Circulation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public Guid MaterialId { get; set; }
    public Guid MemberId { get; set; }
    public string? TrackingId { get; set; }
    public string? MemberName { get; set; }
    public DateTime IssuedOn { get; set; } = DateTime.UtcNow;
    public DateTime DueOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
    public CirculationStatus Status { get; set; } = CirculationStatus.Issued;
    public decimal LateFee { get; set; }
    public int ReissueCount { get; set; }
    public string? CreatedBy { get; set; }
}
