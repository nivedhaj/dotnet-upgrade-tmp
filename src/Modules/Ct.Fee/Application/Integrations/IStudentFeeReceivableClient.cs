using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ct.Fee.Application.Integrations;

/// <summary>Port of StudentServiceClient.UpdateStudentFeeReceivable.</summary>
public interface IStudentFeeReceivableClient
{
    Task UpdateAsync(string studentId, Guid ownerId, Guid feeId, string feeName, Guid txId, decimal balance, CancellationToken ct = default);
}

public sealed class NullStudentFeeReceivableClient : IStudentFeeReceivableClient
{
    public Task UpdateAsync(string studentId, Guid ownerId, Guid feeId, string feeName, Guid txId, decimal balance, CancellationToken ct = default)
        => Task.CompletedTask;
}

public sealed class HttpStudentFeeReceivableClient(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration config,
    ILogger<HttpStudentFeeReceivableClient> logger) : IStudentFeeReceivableClient
{
    public async Task UpdateAsync(
        string studentId, Guid ownerId, Guid feeId, string feeName, Guid txId, decimal balance, CancellationToken ct = default)
    {
        var baseUrl = config["Fee:StudentService:BaseUrl"] ?? config["Services:Stu:BaseUrl"] ?? "";
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            logger.LogDebug("Student service base URL not configured; skipping fee receivable update.");
            return;
        }

        try
        {
            var client = httpClientFactory.CreateClient("Stu");
            var payload = new
            {
                Id = studentId,
                InstId = ownerId.ToString(),
                FeeId = feeId.ToString(),
                FeeName = feeName,
                TxId = txId.ToString(),
                Amount = balance
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/api/stu/student/feesReceivable")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            var ctx = httpContextAccessor.HttpContext;
            if (ctx?.Request.Headers.TryGetValue("AccessCode", out var code) == true)
                request.Headers.TryAddWithoutValidation("AccessCode", code.ToString());
            if (ctx?.Request.Headers.TryGetValue("AccessToken", out var token) == true)
                request.Headers.TryAddWithoutValidation("AccessToken", token.ToString());

            var response = await client.SendAsync(request, ct);
            logger.LogInformation("Student fee receivable update for {StudentId}: {Status}", studentId, response.StatusCode);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update student fee receivable for {StudentId}", studentId);
            // Original logged and continued — do not fail the transaction
        }
    }
}
