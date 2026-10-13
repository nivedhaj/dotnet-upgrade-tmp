namespace Ct.Fee.Domain;

public interface ITenantScoped
{
    Guid OwnerId { get; set; }
}

public abstract class AuditableEntity : ITenantScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid? ParentId { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTimeOffset ModifiedOn { get; set; }
    public string ModifiedBy { get; set; } = "";
    public uint Version { get; set; }
}

public enum FeeStatus { Draft = 0, Active = 1, Inactive = 2, Archived = 3 }
public enum TransactionStatus { Pending = 0, Completed = 1, Cancelled = 2, Reversed = 3 }
public enum SuspenseStatus { Unidentified = 0, PartiallyAllocated = 1, Allocated = 2, Reversed = 3 }
public enum RefundStatus { Pending = 0, Approved = 1, Rejected = 2, Paid = 3, Cancelled = 4 }
