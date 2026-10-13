namespace Ct.Fee.Application.Integrations;

/// <summary>
/// Full port of original RabbitMQService enqueue surface used by ct.fee.
/// </summary>
public interface IFeeEventPublisher
{
    // ── Fee domain (exchange ct_xt, topic) ───────────────────────────────────
    Task PublishFeeCreatedAsync(Guid feeId, string? token = null, CancellationToken ct = default);
    Task PublishFeeStructureUpdatedAsync(Guid feeId, string? token = null, CancellationToken ct = default);
    Task PublishFeeTxDoneAsync(Guid feeTxId, string? token = null, CancellationToken ct = default);
    Task PublishFeeTxStatusUpdatedAsync(Guid feeTxId, string status, string? token = null, CancellationToken ct = default);
    Task PublishStudentFeesReceivableAsync(string studentId, Guid feeId, Guid? txId, decimal balance, string? token = null, CancellationToken ct = default);

    // ── Generic jobs (exchange ct_x, direct) ─────────────────────────────────
    Task PublishEmailAsync(EmailJobMessage email, CancellationToken ct = default);
    Task PublishNotificationAsync(NotificationJobMessage notification, CancellationToken ct = default);
    Task PublishSmsAsync(SmsJobMessage sms, CancellationToken ct = default);
    Task PublishJobAsync(FeeJobMessage job, CancellationToken ct = default);
    Task PublishLogAsync(string message, CancellationToken ct = default);
    Task PublishEtlScriptAsync(FeeJobMessage job, CancellationToken ct = default);
}
