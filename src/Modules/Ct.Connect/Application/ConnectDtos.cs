namespace Ct.Connect.Application;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public sealed class SmsUsageDto
{
    public string? OwnerId { get; set; }
    public DateTime? Date { get; set; }
    public long Count { get; set; }
    public long Credits { get; set; }
}

public sealed class EmailUsageDto
{
    public string? OwnerId { get; set; }
    public DateTime? Date { get; set; }
    public long Count { get; set; }
}

public sealed class SmsMessageDto
{
    public string Id { get; set; } = "";
    public string? OwnerId { get; set; }
    public string Message { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTimeOffset? CreatedOn { get; set; }
}

public sealed class SmsDto
{
    public string Id { get; set; } = "";
    public string Message { get; set; } = "";
    public string? SmsRefId { get; set; }
    public string Gateway { get; set; } = "";
    public IReadOnlyList<SmsRecipientDto> Recipients { get; set; } = [];
    public DateTimeOffset? CreatedOn { get; set; }
}

public sealed class SmsRecipientDto
{
    public string? Name { get; set; }
    public string Mobile { get; set; } = "";
    public string? Status { get; set; }
}

public sealed class EmailDto
{
    public string Id { get; set; } = "";
    public string Subject { get; set; } = "";
    public string From { get; set; } = "";
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public IReadOnlyList<string> Recipients { get; set; } = [];
    public DateTimeOffset? CreatedOn { get; set; }
}

public sealed class TopicDto
{
    public string Id { get; set; } = "";
    public string? OwnerId { get; set; }
    public string Name { get; set; } = "";
    public string? Handle { get; set; }
    public string? FriendlyName { get; set; }
    public string? Description { get; set; }
    public string? MainTopicId { get; set; }
    public string Role { get; set; } = "";
    public string Category { get; set; } = "";
    public string Access { get; set; } = "";
    public string Status { get; set; } = "";
    public bool CanUnsubscribe { get; set; }
    public bool CanPublish { get; set; }
    public bool IsSubscriptionAllowed { get; set; }
    public IReadOnlyList<string>? Tags { get; set; }
}

public sealed class CreateTopicRequest
{
    public string? OwnerId { get; set; }
    public required string Name { get; set; }
    public string? FriendlyName { get; set; }
    public string? Description { get; set; }
    public string? MainTopicId { get; set; }
    public string Role { get; set; } = "Member";
    public string Category { get; set; } = "PrivateToInstitute";
    public string Access { get; set; } = "Open";
    public bool CanUnsubscribe { get; set; } = true;
    public bool CanPublish { get; set; }
    public bool IsSubscriptionAllowed { get; set; } = true;
    public List<string>? Tags { get; set; }
}

public sealed class SendSmsRequest
{
    public required string Message { get; set; }
    public List<SmsRecipientDto> Recipients { get; set; } = [];
    public string? SenderId { get; set; }
    public string? SmsRefId { get; set; }
}

public sealed class SendEmailRequest
{
    public string Type { get; set; } = "generic";
    public string? From { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }
    public List<string> Recipients { get; set; } = [];
}

public sealed class ContactLookupRequest
{
    public List<string> Contacts { get; set; } = [];
}

public sealed class ContactLookupDto
{
    public string Contact { get; set; } = "";
    public bool IsValid { get; set; }
    public string? Normalized { get; set; }
    public string Kind { get; set; } = ""; // mobile | email
}

public sealed class ApprovalRequest
{
    public required string Message { get; set; }
    public string? InstId { get; set; }
    public bool Approve { get; set; }
}

public sealed class IdRequest
{
    public required string Id { get; set; }
}

public sealed class DisapproveRequest
{
    public required string Id { get; set; }
    public string? Reason { get; set; }
}

public sealed class UpdateTopicFieldRequest
{
    public required string Id { get; set; }
    public string? Value { get; set; }
}

public sealed class UpdateSubscriptionsRequest
{
    public required string Id { get; set; }
    public Dictionary<string, object>? Subscriptions { get; set; }
}

public sealed class HandleRequest
{
    public required string Id { get; init; }
    public required string Handle { get; init; }
}

public sealed class SubscriptionAllowedRequest
{
    public required string Id { get; init; }
    public bool IsSubscriptionAllowed { get; init; }
}

public sealed class FacetApplyRequest
{
    public List<FacetItem> Facets { get; set; } = [];
}

public sealed class FacetItem
{
    public string? Key { get; set; }
    public List<string> Values { get; set; } = [];
}

public sealed class WorkflowInitResponse
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? OwnerId { get; set; }
    public List<WorkflowJobDto> Jobs { get; set; } = [];
}

public sealed class WorkflowJobDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Unknown";
}

// ── Original RecipientPostData / CreateEmailDto shapes ───────────────────────

public sealed class RecipientPostData
{
    public RecipientsFacet Recipients { get; set; } = new();
    public string? Text { get; set; }
    public string? SMSRefId { get; set; }
}

public sealed class RecipientsFacet
{
    public List<string> Manual { get; set; } = [];
    public List<List<string>>? Parent { get; set; }
    public List<List<string>>? Student { get; set; }
}

public sealed class CreateEmailDto
{
    public string Type { get; set; } = "generic";
    public string? From { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string? Html { get; set; }
    public RecipientsFacet Recipients { get; set; } = new();
    public List<string>? To { get; set; }
}

public sealed class ConnectJobMessage
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string title { get; set; } = "";
    public object? payload { get; set; }
    public string? token { get; set; }
    public string? user_id { get; set; }
    public string? client_id { get; set; }
    public string? topic { get; set; }
}

public sealed class SmsEnqueuePayload
{
    public List<string> mobiles { get; set; } = [];
    public string message { get; set; } = "";
    public string? sender_id { get; set; }
    public string? sms_id { get; set; }
    public string? inst_id { get; set; }
}

public sealed class EmailEnqueuePayload
{
    public string? subject { get; set; }
    public string? from { get; set; }
    public List<string> to { get; set; } = [];
    public string? html { get; set; }
    public string? inst_id { get; set; }
    public string? inst_short_name { get; set; }
    public string? sender_id { get; set; }
    public List<object>? attachments { get; set; }
}
