namespace Ct.Shared.Domain;

public interface ITenantScoped { Guid OwnerId { get; set; } }

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
