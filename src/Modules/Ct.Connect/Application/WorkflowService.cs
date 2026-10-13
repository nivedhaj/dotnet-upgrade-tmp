using System.Text.Json;
using Ct.Connect.Security;
using Microsoft.Extensions.Logging;

namespace Ct.Connect.Application;

public sealed class WorkflowService(
    IUserContextAccessor ucAccessor,
    IConnectEventPublisher events,
    ILogger<WorkflowService> logger)
{
    public WorkflowInitResponse? For(string title)
    {
        var owner = ucAccessor.Current?.InstId ?? "";
        if (title.Equals("email_generic", StringComparison.OrdinalIgnoreCase))
            return Build(owner, "email_generic", "email_set_from", "email_set_subject", "email_set_body", "email_get_recipients", "email_get_attachments");
        if (title.Equals("email_markscard", StringComparison.OrdinalIgnoreCase))
            return Build(owner, "email_markscard", "email_set_from", "email_set_subject", "email_set_body", "email_get_recipients");
        if (title.Equals("sms_generic", StringComparison.OrdinalIgnoreCase))
            return Build(owner, "sms_generic", "sms_set_message", "sms_get_recipients", "sms_send");
        return null;
    }

    public async Task PushAsync(JsonElement payload, CancellationToken ct = default)
    {
        logger.LogInformation("Workflow push");
        await events.PublishWorkflowAsync(payload, ct);
    }

    private static WorkflowInitResponse Build(string ownerId, string title, params string[] jobs) => new()
    {
        Id = Guid.NewGuid().ToString("D"),
        Title = title,
        OwnerId = ownerId,
        Jobs = jobs.Select((j, i) => new WorkflowJobDto { Id = i + 1, Title = j, Status = "Unknown" }).ToList()
    };
}
