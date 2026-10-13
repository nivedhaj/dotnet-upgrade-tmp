namespace Ct.Connect.Domain;

public enum RoleEnum
{
    Admin = 10,
    Member = 20
}

public enum CategoryEnum
{
    PrivateToInstitute = 30,
    Public = 40
}

public enum AccessEnum
{
    Open = 50,
    Restricted = 60
}

public enum TopicStatusEnum
{
    Unknown = 0,
    Active = 1,
    Disabled = 99
}

public class Topic : Entity
{
    public string? MainTopicId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Handle { get; set; }

    public string? FriendlyName { get; set; }

    public string? Description { get; set; }

    public RoleEnum Role { get; set; }

    public CategoryEnum Category { get; set; }

    public AccessEnum Access { get; set; }

    /// <summary>Stored as JSONB.</summary>
    public Dictionary<string, object>? Subscriptions { get; set; }

    public TopicStatusEnum Status { get; set; } = TopicStatusEnum.Active;

    /// <summary>Stored as JSONB.</summary>
    public Dictionary<string, string>? Meta { get; set; }

    /// <summary>Stored as JSONB.</summary>
    public List<string>? Tags { get; set; }

    public bool CanUnsubscribe { get; set; }

    public bool CanPublish { get; set; }

    public bool IsSubscriptionAllowed { get; set; }
}
