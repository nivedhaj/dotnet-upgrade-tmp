using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ct.Fee.Application.Integrations;

// ─── Job / DTO contracts (original snake_case shapes) ────────────────────────

public sealed class FeeJobMessage
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string title { get; set; } = "";
    public object? payload { get; set; }
    public object? meta { get; set; }
    public string? user_id { get; set; }
    public string? token { get; set; }
}

public sealed class EmailAttachmentMessage
{
    public string file_path { get; set; } = "";
    public string mime_type { get; set; } = "application";
    public string mime_sub_type { get; set; } = "pdf";
    public string? data { get; set; }
}

public sealed class EmailJobMessage
{
    public string? subject { get; set; }
    public string? from { get; set; }
    public List<string> to { get; set; } = [];
    public string? html { get; set; }
    public string? sender_id { get; set; }
    public string? inst_id { get; set; }
    public string? inst_short_name { get; set; }
    public List<EmailAttachmentMessage> attachments { get; set; } = [];
}

public sealed class NotificationJobMessage
{
    public string? title { get; set; }
    public string? body { get; set; }
    public List<string> recipients { get; set; } = [];
    public object? data { get; set; }
}

public sealed class SmsJobMessage
{
    public string? message { get; set; }
    public List<string> to { get; set; } = [];
    public string? sender_id { get; set; }
    public string? inst_id { get; set; }
}

// ─── Options ─────────────────────────────────────────────────────────────────

public sealed class RabbitMqOptions
{
    public const string SectionName = "Fee:RabbitMq";

    public bool Enabled { get; set; }
    public string HostName { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; } = 5672;

    public string FeeTopicExchange { get; set; } = "ct_xt";
    public string DefaultExchange { get; set; } = "ct_x";

    public string FeeCreatedRoutingKey { get; set; } = "fee.created";
    public string FeeUpdatedRoutingKey { get; set; } = "fee.updated";
    public string FeeTxDoneRoutingKey { get; set; } = "fee.tx.done";
    public string FeeTxStatusUpdatedRoutingKey { get; set; } = "fee.tx.status.updated";
    public string StudentFeesReceivableRoutingKey { get; set; } = "student.fees.receivable";

    public string EmailRoutingKey { get; set; } = "ct_r_email_generic";
    public string NotificationRoutingKey { get; set; } = "ct_r_push_notification";
    public string SmsRoutingKey { get; set; } = "ct_r_sms";
    public string JobsRoutingKey { get; set; } = "ct_r_jobs";
    public string LogsRoutingKey { get; set; } = "ct_r_logs";
    public string EtlRoutingKey { get; set; } = "ct_r_etl_worker";

    public bool DeclareTopology { get; set; } = true;
    public string? FeeTxDoneQueue { get; set; } = "ct_q_fee_tx_done";
    public string? FeeEventsQueue { get; set; } = "ct_q_fee_events";
    public string? EmailQueue { get; set; } = "ct_q_email";
}

// ─── Connection ──────────────────────────────────────────────────────────────

public interface IRabbitMqConnection : IDisposable
{
    bool IsConnected { get; }
    IModel CreateChannel();
    bool TryConnect();
}

public sealed class RabbitMqConnection : IRabbitMqConnection
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly object _sync = new();
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public bool TryConnect()
    {
        if (!_options.Enabled) return false;
        lock (_sync)
        {
            if (IsConnected) return true;
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    VirtualHost = _options.VirtualHost,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    Port = _options.Port,
                    DispatchConsumersAsync = true,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };
                _connection = factory.CreateConnection("ct.fee");
                _logger.LogInformation("RabbitMQ connected {Host}:{Port} vhost={VHost}",
                    _options.HostName, _options.Port, _options.VirtualHost);
                if (_options.DeclareTopology) EnsureTopology();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ connection failed");
                return false;
            }
        }
    }

    private void EnsureTopology()
    {
        using var ch = _connection!.CreateModel();
        ch.ExchangeDeclare(_options.FeeTopicExchange, ExchangeType.Topic, durable: true);
        ch.ExchangeDeclare(_options.DefaultExchange, ExchangeType.Direct, durable: true);

        void Bind(string? queue, string exchange, string routingKey)
        {
            if (string.IsNullOrWhiteSpace(queue)) return;
            ch.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            ch.QueueBind(queue, exchange, routingKey);
        }

        Bind(_options.FeeTxDoneQueue, _options.FeeTopicExchange, _options.FeeTxDoneRoutingKey);
        if (!string.IsNullOrWhiteSpace(_options.FeeEventsQueue))
        {
            ch.QueueDeclare(_options.FeeEventsQueue, durable: true, exclusive: false, autoDelete: false);
            foreach (var rk in new[]
            {
                _options.FeeCreatedRoutingKey,
                _options.FeeUpdatedRoutingKey,
                _options.FeeTxDoneRoutingKey,
                _options.FeeTxStatusUpdatedRoutingKey,
                _options.StudentFeesReceivableRoutingKey
            })
                ch.QueueBind(_options.FeeEventsQueue, _options.FeeTopicExchange, rk);
        }
        Bind(_options.EmailQueue, _options.DefaultExchange, _options.EmailRoutingKey);

        _logger.LogInformation("RabbitMQ topology ready (exchanges {Topic}/{Direct})",
            _options.FeeTopicExchange, _options.DefaultExchange);
    }

    public IModel CreateChannel()
    {
        if (!IsConnected && !TryConnect())
            throw new InvalidOperationException("RabbitMQ is not connected.");
        return _connection!.CreateModel();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _connection?.Close(); _connection?.Dispose(); }
        catch (Exception ex) { _logger.LogWarning(ex, "RabbitMQ dispose error"); }
    }
}

// ─── Low-level publish ───────────────────────────────────────────────────────

public interface IRabbitMqPublisher
{
    void Publish(object message, string routingKey, string exchange);
}

public sealed class RabbitMqPublisher(
    IRabbitMqConnection connection,
    ILogger<RabbitMqPublisher> logger) : IRabbitMqPublisher
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public void Publish(object message, string routingKey, string exchange)
    {
        if (!connection.IsConnected && !connection.TryConnect())
        {
            logger.LogWarning("RabbitMQ down; drop routingKey={RoutingKey}", routingKey);
            return;
        }

        using var channel = connection.CreateChannel();
        var props = channel.CreateBasicProperties();
        props.DeliveryMode = 2;
        props.ContentType = "application/json";
        props.MessageId = Guid.NewGuid().ToString();
        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var exchangeType = routingKey.Contains('.') ? ExchangeType.Topic : ExchangeType.Direct;
        channel.ExchangeDeclare(exchange, exchangeType, durable: true);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, message.GetType(), JsonOpts));
        channel.BasicPublish(exchange, routingKey, mandatory: false, basicProperties: props, body: body);
        logger.LogInformation("Published exchange={Exchange} rk={RoutingKey} bytes={Len}",
            exchange, routingKey, body.Length);
    }
}

// ─── Full event publisher ────────────────────────────────────────────────────

public sealed class LoggingFeeEventPublisher(ILogger<LoggingFeeEventPublisher> logger) : IFeeEventPublisher
{
    public Task PublishFeeCreatedAsync(Guid feeId, string? token = null, CancellationToken ct = default)
    {
        logger.LogInformation("fee.created (log): {FeeId}", feeId);
        return Task.CompletedTask;
    }

    public Task PublishFeeStructureUpdatedAsync(Guid feeId, string? token = null, CancellationToken ct = default)
    {
        logger.LogInformation("fee.updated (log): {FeeId}", feeId);
        return Task.CompletedTask;
    }

    public Task PublishFeeTxDoneAsync(Guid feeTxId, string? token = null, CancellationToken ct = default)
    {
        logger.LogInformation("fee.tx.done (log): {FeeTxId}", feeTxId);
        return Task.CompletedTask;
    }

    public Task PublishFeeTxStatusUpdatedAsync(Guid feeTxId, string status, string? token = null, CancellationToken ct = default)
    {
        logger.LogInformation("fee.tx.status.updated (log): {FeeTxId} {Status}", feeTxId, status);
        return Task.CompletedTask;
    }

    public Task PublishStudentFeesReceivableAsync(string studentId, Guid feeId, Guid? txId, decimal balance, string? token = null, CancellationToken ct = default)
    {
        logger.LogInformation("student.fees.receivable (log): {StudentId} {FeeId} {Balance}", studentId, feeId, balance);
        return Task.CompletedTask;
    }

    public Task PublishEmailAsync(EmailJobMessage email, CancellationToken ct = default)
    {
        logger.LogInformation("email (log): to={To} subject={Subject}", string.Join(',', email.to), email.subject);
        return Task.CompletedTask;
    }

    public Task PublishNotificationAsync(NotificationJobMessage notification, CancellationToken ct = default)
    {
        logger.LogInformation("notification (log): {Title}", notification.title);
        return Task.CompletedTask;
    }

    public Task PublishSmsAsync(SmsJobMessage sms, CancellationToken ct = default)
    {
        logger.LogInformation("sms (log): to={To}", string.Join(',', sms.to));
        return Task.CompletedTask;
    }

    public Task PublishJobAsync(FeeJobMessage job, CancellationToken ct = default)
    {
        logger.LogInformation("job (log): {Title}", job.title);
        return Task.CompletedTask;
    }

    public Task PublishLogAsync(string message, CancellationToken ct = default)
    {
        logger.LogInformation("rmq-log: {Message}", message);
        return Task.CompletedTask;
    }

    public Task PublishEtlScriptAsync(FeeJobMessage job, CancellationToken ct = default)
    {
        logger.LogInformation("etl (log): {Title}", job.title);
        return Task.CompletedTask;
    }
}

public sealed class RabbitMqFeeEventPublisher(
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options,
    Ct.Fee.Application.ICurrentUserContext user,
    ILogger<RabbitMqFeeEventPublisher> logger) : IFeeEventPublisher
{
    private RabbitMqOptions O => options.Value;

    private FeeJobMessage Job(string title, object? payload, string? token) => new()
    {
        id = Guid.NewGuid().ToString(),
        title = title,
        payload = payload,
        token = token,
        user_id = user.UserId
    };

    private Task Safe(Action action, string name)
    {
        try { action(); }
        catch (Exception ex) { logger.LogError(ex, "RabbitMQ Error: {Name}", name); }
        return Task.CompletedTask;
    }

    public Task PublishFeeCreatedAsync(Guid feeId, string? token = null, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(Job("fee_created", new { fee_id = feeId.ToString() }, token),
            O.FeeCreatedRoutingKey, O.FeeTopicExchange), "fee.created");

    public Task PublishFeeStructureUpdatedAsync(Guid feeId, string? token = null, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(Job("fee_structure_updated", new { fee_id = feeId.ToString() }, token),
            O.FeeUpdatedRoutingKey, O.FeeTopicExchange), "fee.updated");

    public Task PublishFeeTxDoneAsync(Guid feeTxId, string? token = null, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(Job("fee_tx_done", new { fee_tx_id = feeTxId.ToString() }, token),
            O.FeeTxDoneRoutingKey, O.FeeTopicExchange), "fee.tx.done");

    public Task PublishFeeTxStatusUpdatedAsync(Guid feeTxId, string status, string? token = null, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(Job("fee_tx_status_updated", new { fee_tx_id = feeTxId.ToString(), status }, token),
            O.FeeTxStatusUpdatedRoutingKey, O.FeeTopicExchange), "fee.tx.status.updated");

    public Task PublishStudentFeesReceivableAsync(string studentId, Guid feeId, Guid? txId, decimal balance, string? token = null, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(Job("student_fees_receivable", new
        {
            student_id = studentId,
            fee_id = feeId.ToString(),
            tx_id = txId?.ToString(),
            amount = balance
        }, token), O.StudentFeesReceivableRoutingKey, O.FeeTopicExchange), "student.fees.receivable");

    public Task PublishEmailAsync(EmailJobMessage email, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(email, O.EmailRoutingKey, O.DefaultExchange), "email");

    public Task PublishNotificationAsync(NotificationJobMessage notification, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(notification, O.NotificationRoutingKey, O.DefaultExchange), "notification");

    public Task PublishSmsAsync(SmsJobMessage sms, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(sms, O.SmsRoutingKey, O.DefaultExchange), "sms");

    public Task PublishJobAsync(FeeJobMessage job, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(job, O.JobsRoutingKey, O.DefaultExchange), "job");

    public Task PublishLogAsync(string message, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(message, O.LogsRoutingKey, O.DefaultExchange), "log");

    public Task PublishEtlScriptAsync(FeeJobMessage job, CancellationToken ct = default) =>
        Safe(() => publisher.Publish(job, O.EtlRoutingKey, O.DefaultExchange), "etl");
}

public sealed class ResilientFeeEventPublisher(
    IFeeEventPublisher inner,
    ILogger<ResilientFeeEventPublisher> logger) : IFeeEventPublisher
{
    private async Task Guard(Func<Task> f, string name)
    {
        try { await f(); }
        catch (Exception ex) { logger.LogError(ex, "Event {Name} failed; continuing", name); }
    }

    public Task PublishFeeCreatedAsync(Guid feeId, string? token = null, CancellationToken ct = default) =>
        Guard(() => inner.PublishFeeCreatedAsync(feeId, token, ct), nameof(PublishFeeCreatedAsync));
    public Task PublishFeeStructureUpdatedAsync(Guid feeId, string? token = null, CancellationToken ct = default) =>
        Guard(() => inner.PublishFeeStructureUpdatedAsync(feeId, token, ct), nameof(PublishFeeStructureUpdatedAsync));
    public Task PublishFeeTxDoneAsync(Guid feeTxId, string? token = null, CancellationToken ct = default) =>
        Guard(() => inner.PublishFeeTxDoneAsync(feeTxId, token, ct), nameof(PublishFeeTxDoneAsync));
    public Task PublishFeeTxStatusUpdatedAsync(Guid feeTxId, string status, string? token = null, CancellationToken ct = default) =>
        Guard(() => inner.PublishFeeTxStatusUpdatedAsync(feeTxId, status, token, ct), nameof(PublishFeeTxStatusUpdatedAsync));
    public Task PublishStudentFeesReceivableAsync(string studentId, Guid feeId, Guid? txId, decimal balance, string? token = null, CancellationToken ct = default) =>
        Guard(() => inner.PublishStudentFeesReceivableAsync(studentId, feeId, txId, balance, token, ct), nameof(PublishStudentFeesReceivableAsync));
    public Task PublishEmailAsync(EmailJobMessage email, CancellationToken ct = default) =>
        Guard(() => inner.PublishEmailAsync(email, ct), nameof(PublishEmailAsync));
    public Task PublishNotificationAsync(NotificationJobMessage notification, CancellationToken ct = default) =>
        Guard(() => inner.PublishNotificationAsync(notification, ct), nameof(PublishNotificationAsync));
    public Task PublishSmsAsync(SmsJobMessage sms, CancellationToken ct = default) =>
        Guard(() => inner.PublishSmsAsync(sms, ct), nameof(PublishSmsAsync));
    public Task PublishJobAsync(FeeJobMessage job, CancellationToken ct = default) =>
        Guard(() => inner.PublishJobAsync(job, ct), nameof(PublishJobAsync));
    public Task PublishLogAsync(string message, CancellationToken ct = default) =>
        Guard(() => inner.PublishLogAsync(message, ct), nameof(PublishLogAsync));
    public Task PublishEtlScriptAsync(FeeJobMessage job, CancellationToken ct = default) =>
        Guard(() => inner.PublishEtlScriptAsync(job, ct), nameof(PublishEtlScriptAsync));
}
