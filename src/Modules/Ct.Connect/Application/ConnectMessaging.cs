using System.Text;
using System.Text.Json;
using Ct.Connect.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ct.Connect.Application;

public sealed class ConnectRabbitMqOptions
{
    public const string SectionName = "Connect:RabbitMq";
    public bool Enabled { get; set; }
    public string HostName { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; } = 5672;
    public string DefaultExchange { get; set; } = "ct_x";
    public string TopicExchange { get; set; } = "ct_xt";
    public string EmailRoutingKey { get; set; } = "ct_r_email_generic";
    public string SmsRoutingKey { get; set; } = "ct_r_sms";
    public string WorkflowRoutingKey { get; set; } = "ct_r_workflow";
    public string TopicUpdatedRoutingKey { get; set; } = "topic.updated";
    public string SmsDeliveryRoutingKey { get; set; } = "ct_r_sms_delivery_data";
    public string EmailDeliveryRoutingKey { get; set; } = "ct_r_email_delivery_data";
    public bool DeclareTopology { get; set; } = true;
}

public interface IConnectEventPublisher
{
    Task PublishSmsJobAsync(object job, CancellationToken ct = default);
    Task PublishEmailJobAsync(object job, CancellationToken ct = default);
    Task PublishWorkflowAsync(object payload, CancellationToken ct = default);
    Task PublishTopicUpdatedAsync(string instId, CancellationToken ct = default);
    Task PublishSmsDeliveryDataJobAsync(object job, CancellationToken ct = default);
    Task PublishEmailDeliveryDataJobAsync(object job, CancellationToken ct = default);
}

public sealed class LoggingConnectEventPublisher(ILogger<LoggingConnectEventPublisher> logger) : IConnectEventPublisher
{
    public Task PublishSmsJobAsync(object job, CancellationToken ct = default)
    { logger.LogInformation("SMS job (log): {Job}", JsonSerializer.Serialize(job)); return Task.CompletedTask; }
    public Task PublishEmailJobAsync(object job, CancellationToken ct = default)
    { logger.LogInformation("Email job (log): {Job}", JsonSerializer.Serialize(job)); return Task.CompletedTask; }
    public Task PublishWorkflowAsync(object payload, CancellationToken ct = default)
    { logger.LogInformation("Workflow (log): {Payload}", JsonSerializer.Serialize(payload)); return Task.CompletedTask; }
    public Task PublishTopicUpdatedAsync(string instId, CancellationToken ct = default)
    { logger.LogInformation("topic.updated (log): {InstId}", instId); return Task.CompletedTask; }
    public Task PublishSmsDeliveryDataJobAsync(object job, CancellationToken ct = default)
    { logger.LogInformation("SMS delivery job (log)"); return Task.CompletedTask; }
    public Task PublishEmailDeliveryDataJobAsync(object job, CancellationToken ct = default)
    { logger.LogInformation("Email delivery job (log)"); return Task.CompletedTask; }
}

public sealed class RabbitMqConnectEventPublisher : IConnectEventPublisher, IDisposable
{
    private readonly ConnectRabbitMqOptions _opt;
    private readonly ILogger<RabbitMqConnectEventPublisher> _logger;
    private readonly object _sync = new();
    private IConnection? _connection;

    public RabbitMqConnectEventPublisher(IOptions<ConnectRabbitMqOptions> options, ILogger<RabbitMqConnectEventPublisher> logger)
    {
        _opt = options.Value;
        _logger = logger;
    }

    private bool TryConnect()
    {
        if (!_opt.Enabled) return false;
        lock (_sync)
        {
            if (_connection is { IsOpen: true }) return true;
            try
            {
                var f = new ConnectionFactory
                {
                    HostName = _opt.HostName, VirtualHost = _opt.VirtualHost,
                    UserName = _opt.UserName, Password = _opt.Password, Port = _opt.Port,
                    AutomaticRecoveryEnabled = true
                };
                _connection = f.CreateConnection("ct.connect");
                if (_opt.DeclareTopology)
                {
                    using var ch = _connection.CreateModel();
                    ch.ExchangeDeclare(_opt.DefaultExchange, ExchangeType.Direct, durable: true);
                    ch.ExchangeDeclare(_opt.TopicExchange, ExchangeType.Topic, durable: true);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connect RabbitMQ connect failed");
                return false;
            }
        }
    }

    private void Publish(object message, string routingKey, string exchange)
    {
        if (!TryConnect() || _connection is null)
        {
            _logger.LogWarning("RabbitMQ unavailable; drop rk={Rk}", routingKey);
            return;
        }
        using var ch = _connection.CreateModel();
        var props = ch.CreateBasicProperties();
        props.DeliveryMode = 2;
        props.ContentType = "application/json";
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var type = routingKey.Contains('.') ? ExchangeType.Topic : ExchangeType.Direct;
        ch.ExchangeDeclare(exchange, type, durable: true);
        ch.BasicPublish(exchange, routingKey, props, body);
        _logger.LogInformation("Published rk={Rk} exchange={Ex}", routingKey, exchange);
    }

    public Task PublishSmsJobAsync(object job, CancellationToken ct = default)
    { Publish(job, _opt.SmsRoutingKey, _opt.DefaultExchange); return Task.CompletedTask; }
    public Task PublishEmailJobAsync(object job, CancellationToken ct = default)
    { Publish(job, _opt.EmailRoutingKey, _opt.DefaultExchange); return Task.CompletedTask; }
    public Task PublishWorkflowAsync(object payload, CancellationToken ct = default)
    { Publish(payload, _opt.WorkflowRoutingKey, _opt.DefaultExchange); return Task.CompletedTask; }
    public Task PublishTopicUpdatedAsync(string instId, CancellationToken ct = default)
    {
        Publish(new { id = Guid.NewGuid().ToString(), title = "topic_updated", payload = new { inst_id = instId } },
            _opt.TopicUpdatedRoutingKey, _opt.TopicExchange);
        return Task.CompletedTask;
    }
    public Task PublishSmsDeliveryDataJobAsync(object job, CancellationToken ct = default)
    { Publish(job, _opt.SmsDeliveryRoutingKey, _opt.DefaultExchange); return Task.CompletedTask; }
    public Task PublishEmailDeliveryDataJobAsync(object job, CancellationToken ct = default)
    { Publish(job, _opt.EmailDeliveryRoutingKey, _opt.DefaultExchange); return Task.CompletedTask; }

    public void Dispose()
    {
        try { _connection?.Close(); _connection?.Dispose(); } catch { /* ignore */ }
    }
}

public static class SmsCreditHelper
{
    public static int CreditsPerMessage(int messageLength)
    {
        if (messageLength <= 0) return 0;
        if (messageLength <= 160) return 1;
        if (messageLength <= 306) return 2;
        if (messageLength <= 459) return 3;
        return (int)Math.Ceiling(messageLength / 153.0);
    }
}
