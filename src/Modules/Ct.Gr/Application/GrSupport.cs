using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ct.Gr.Application;

public sealed class GrRabbitMqOptions
{
    public const string SectionName = "Gr:RabbitMq";
    public bool Enabled { get; set; }
    public string HostName { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; } = 5672;
    public string Exchange { get; set; } = "ct_xt";
    public string ReceiptCreatedRoutingKey { get; set; } = "gr.receipt.created";
    public string ReceiptUpdatedRoutingKey { get; set; } = "gr.receipt.updated";
    public string ReceiptPrintedRoutingKey { get; set; } = "gr.receipt.printed";
}

public interface IGrEventPublisher
{
    Task PublishReceiptCreatedAsync(Guid id, string number, CancellationToken ct = default);
    Task PublishReceiptUpdatedAsync(Guid id, string number, string action, CancellationToken ct = default);
    Task PublishReceiptPrintedAsync(Guid id, string number, CancellationToken ct = default);
}

public sealed class LoggingGrEventPublisher(ILogger<LoggingGrEventPublisher> logger) : IGrEventPublisher
{
    public Task PublishReceiptCreatedAsync(Guid id, string number, CancellationToken ct = default)
    { logger.LogInformation("gr.receipt.created {Id} {Number}", id, number); return Task.CompletedTask; }
    public Task PublishReceiptUpdatedAsync(Guid id, string number, string action, CancellationToken ct = default)
    { logger.LogInformation("gr.receipt.updated {Id} {Action}", id, action); return Task.CompletedTask; }
    public Task PublishReceiptPrintedAsync(Guid id, string number, CancellationToken ct = default)
    { logger.LogInformation("gr.receipt.printed {Id} {Number}", id, number); return Task.CompletedTask; }
}

/// <summary>Optional real RabbitMQ — enabled via Gr:RabbitMq:Enabled (or Fee/Connect shared flags).</summary>
public sealed class RabbitMqGrEventPublisher : IGrEventPublisher, IDisposable
{
    private readonly GrRabbitMqOptions _opt;
    private readonly ILogger<RabbitMqGrEventPublisher> _logger;
    private RabbitMQ.Client.IConnection? _conn;
    private readonly object _sync = new();

    public RabbitMqGrEventPublisher(IOptions<GrRabbitMqOptions> options, ILogger<RabbitMqGrEventPublisher> logger)
    {
        _opt = options.Value;
        _logger = logger;
    }

    private bool TryConnect()
    {
        if (!_opt.Enabled) return false;
        lock (_sync)
        {
            if (_conn is { IsOpen: true }) return true;
            try
            {
                var f = new RabbitMQ.Client.ConnectionFactory
                {
                    HostName = _opt.HostName, VirtualHost = _opt.VirtualHost,
                    UserName = _opt.UserName, Password = _opt.Password, Port = _opt.Port,
                    AutomaticRecoveryEnabled = true
                };
                _conn = f.CreateConnection("ct.gr");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GR RabbitMQ connect failed");
                return false;
            }
        }
    }

    private void Publish(object payload, string rk)
    {
        if (!TryConnect() || _conn is null) { _logger.LogWarning("drop {Rk}", rk); return; }
        using var ch = _conn.CreateModel();
        ch.ExchangeDeclare(_opt.Exchange, RabbitMQ.Client.ExchangeType.Topic, durable: true);
        var props = ch.CreateBasicProperties();
        props.DeliveryMode = 2;
        props.ContentType = "application/json";
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        ch.BasicPublish(_opt.Exchange, rk, props, body);
    }

    public Task PublishReceiptCreatedAsync(Guid id, string number, CancellationToken ct = default)
    {
        Publish(new { id = Guid.NewGuid().ToString(), title = "gr_receipt_created", payload = new { receipt_id = id, number } },
            _opt.ReceiptCreatedRoutingKey);
        return Task.CompletedTask;
    }

    public Task PublishReceiptUpdatedAsync(Guid id, string number, string action, CancellationToken ct = default)
    {
        Publish(new { id = Guid.NewGuid().ToString(), title = "gr_receipt_updated", payload = new { receipt_id = id, number, action } },
            _opt.ReceiptUpdatedRoutingKey);
        return Task.CompletedTask;
    }

    public Task PublishReceiptPrintedAsync(Guid id, string number, CancellationToken ct = default)
    {
        Publish(new { id = Guid.NewGuid().ToString(), title = "gr_receipt_printed", payload = new { receipt_id = id, number } },
            _opt.ReceiptPrintedRoutingKey);
        return Task.CompletedTask;
    }

    public void Dispose() { try { _conn?.Close(); _conn?.Dispose(); } catch { /* ignore */ } }
}

public interface IGrPolicy
{
    InstituteBranding GetBranding();
    string FormatReceiptNumber(int sequence, DateTime date);
    bool DefaultInclude80G(string receiptType);
}

public sealed class ConfigGrPolicy(IConfiguration config) : IGrPolicy
{
    public InstituteBranding GetBranding() => new()
    {
        Name = config["Gr:Institute:Name"] ?? config["Institute:Name"] ?? "Institute",
        ShortName = config["Gr:Institute:ShortName"] ?? config["Institute:ShortName"] ?? "INST",
        LogoUrl = config["Gr:Institute:LogoUrl"] ?? config["Institute:LogoUrl"],
        Line1 = config["Gr:Institute:Line1"] ?? config["Institute:Line1"],
        Line2 = config["Gr:Institute:Line2"] ?? config["Institute:Line2"],
        City = config["Gr:Institute:City"] ?? config["Institute:City"],
        Phone = config["Gr:Institute:Phone"] ?? config["Institute:Phone"],
        Email = config["Gr:Institute:Email"] ?? config["Institute:Email"],
        EightyGText = config["Gr:Institute:EightyGText"]
            ?? "Donations are eligible for deduction under section 80G of the Income Tax Act, as applicable."
    };

    public string FormatReceiptNumber(int sequence, DateTime date)
    {
        var pattern = config["Gr:ReceiptNumberFormat"] ?? "{seq:D4}/{year}";
        return pattern
            .Replace("{seq:D4}", sequence.ToString("D4"))
            .Replace("{seq}", sequence.ToString())
            .Replace("{year}", date.Year.ToString())
            .Replace("{shortName}", GetBranding().ShortName);
    }

    public bool DefaultInclude80G(string receiptType) =>
        receiptType.Equals("Donation", StringComparison.OrdinalIgnoreCase)
        || config.GetValue("Gr:DefaultInclude80G", false);
}

public interface IReceiptPrintService
{
    string Render(Domain.Receipt receipt, InstituteBranding branding, bool? include80G = null);
}

public sealed class ReceiptPrintService : IReceiptPrintService
{
    public string Render(Domain.Receipt receipt, InstituteBranding branding, bool? include80G = null)
    {
        var use80G = include80G ?? receipt.Meta.Include80G;
        var addr = string.Join(", ", new[] { branding.Line1, branding.Line2, branding.City }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var items = string.Join("", receipt.OrderItems.Select(i =>
            $"<tr><td>{Enc(i.Particular)}</td><td style='text-align:right'>{i.Amount:0.00}</td></tr>"));
        var fi = receipt.FinancialInstrument is null || receipt.PaymentMode == Domain.PaymentMode.Cash
            ? ""
            : $"<p>Instrument: {Enc(receipt.FinancialInstrument.Number)} / {Enc(receipt.FinancialInstrument.BankName)} {Enc(receipt.FinancialInstrument.BankBranch)} " +
              $"(drawn {receipt.FinancialInstrument.DrawnOn:yyyy-MM-dd})</p>";
        var eighty = use80G
            ? $"<p class='eighty-g'><em>{Enc(branding.EightyGText)}</em></p>"
            : "";
        var logo = string.IsNullOrEmpty(branding.LogoUrl) ? "" : $"<img src=\"{Enc(branding.LogoUrl)}\" alt=\"logo\" style=\"max-height:64px\"/>";

        return $"""
            <!DOCTYPE html><html><head><meta charset="utf-8"/><title>Receipt {Enc(receipt.Number)}</title>
            <style>
              body{{font-family:Segoe UI,Arial,sans-serif;font-size:13px;color:#222}}
              .hdr{{display:flex;gap:12px;align-items:center;border-bottom:2px solid #333;padding-bottom:8px;margin-bottom:12px}}
              table{{width:100%;border-collapse:collapse;margin:12px 0}}
              th,td{{border:1px solid #ccc;padding:6px 8px}}
              th{{background:#f4f4f4;text-align:left}}
              .tot{{font-weight:700}}
              .meta{{color:#555;font-size:12px}}
            </style></head><body>
            <div class="hdr">{logo}<div>
              <div style="font-size:18px;font-weight:700">{Enc(branding.Name)}</div>
              <div class="meta">{Enc(addr)}</div>
              <div class="meta">{Enc(branding.Phone)} {Enc(branding.Email)}</div>
            </div></div>
            <h2>Receipt No. {Enc(receipt.Number)}</h2>
            <p>Date: <strong>{receipt.Date:dd-MMM-yyyy}</strong> &nbsp;|&nbsp; Status: {receipt.Status} &nbsp;|&nbsp; Type: {receipt.ReceiptType}</p>
            <p>Received with thanks from: <strong>{Enc(receipt.Customer.Name)}</strong></p>
            <p class="meta">{Enc(receipt.Customer.Address)} &nbsp; Contact: {Enc(receipt.Customer.Contact)} &nbsp; PAN: {Enc(receipt.Customer.PAN)}</p>
            {(string.IsNullOrEmpty(receipt.Customer.StudyingIn) ? "" : $"<p class='meta'>Class: {Enc(receipt.Customer.StudyingIn)} / {Enc(receipt.Customer.Section)}</p>")}
            <table><thead><tr><th>Particulars</th><th style="width:120px">Amount</th></tr></thead>
            <tbody>{items}</tbody>
            <tfoot><tr class="tot"><td>Total</td><td style="text-align:right">{receipt.TotalAmount:0.00}</td></tr></tfoot></table>
            <p>Payment mode: <strong>{receipt.PaymentMode}</strong></p>
            {fi}
            {eighty}
            <p>Received by: {Enc(receipt.ReceivedBy)}</p>
            {(receipt.Audited ? $"<p class='meta'>Audited on {receipt.AuditedOn:yyyy-MM-dd} by {Enc(receipt.AuditedBy)}</p>" : "")}
            {(receipt.RegulatoryCompliant ? "<p class='meta'>Regulatory compliance: Yes</p>" : "")}
            </body></html>
            """;
    }

    private static string Enc(string? s) => System.Net.WebUtility.HtmlEncode(s ?? "");
}

public static class GrCsv
{
    public static string FromReceipts(IEnumerable<ReceiptDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Number,Date,Customer,Contact,PAN,Type,PaymentMode,Status,TotalAmount,ReceivedBy,Printed,80G");
        foreach (var r in rows)
        {
            sb.Append(Escape(r.Number)).Append(',')
              .Append(r.Date.ToString("yyyy-MM-dd")).Append(',')
              .Append(Escape(r.CustomerName)).Append(',')
              .Append(Escape(r.CustomerContact)).Append(',')
              .Append(Escape(r.CustomerPan)).Append(',')
              .Append(Escape(r.ReceiptType)).Append(',')
              .Append(Escape(r.PaymentMode)).Append(',')
              .Append(Escape(r.Status)).Append(',')
              .Append(r.TotalAmount.ToString("0.00")).Append(',')
              .Append(Escape(r.ReceivedBy)).Append(',')
              .Append(r.Printed ? "Y" : "N").Append(',')
              .Append(r.Include80G ? "Y" : "N").AppendLine();
        }
        return sb.ToString();
    }

    public static string Escape(string? s)
    {
        s ??= "";
        if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
}
