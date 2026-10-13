using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ct.Connect.Application;

public interface IMsAuthService
{
    Task<string?> GetAccessTokenAsync(CancellationToken ct = default);
    bool IsConfigured { get; }
}

public sealed class MsAuthService(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<MsAuthService> logger) : IMsAuthService
{
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(config["Connect:MsGraph:TenantId"])
        && !string.IsNullOrWhiteSpace(config["Connect:MsGraph:ClientId"])
        && !string.IsNullOrWhiteSpace(config["Connect:MsGraph:ClientSecret"]);

    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            logger.LogDebug("MS Graph not configured");
            return null;
        }
        var tenant = config["Connect:MsGraph:TenantId"]!;
        var clientId = config["Connect:MsGraph:ClientId"]!;
        var secret = config["Connect:MsGraph:ClientSecret"]!;
        var client = httpClientFactory.CreateClient("MsGraph");
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = secret,
            ["scope"] = "https://graph.microsoft.com/.default",
            ["grant_type"] = "client_credentials"
        });
        var resp = await client.PostAsync($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token", content, ct);
        if (!resp.IsSuccessStatusCode)
        {
            logger.LogWarning("MS token failed: {Status}", resp.StatusCode);
            return null;
        }
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty("access_token").GetString();
    }
}

public interface IMsTeamService
{
    Task<object> PostChannelMessageAsync(string teamId, string channelId, string message, CancellationToken ct = default);
}

public sealed class MsTeamService(IMsAuthService auth, IHttpClientFactory httpClientFactory, ILogger<MsTeamService> logger) : IMsTeamService
{
    public async Task<object> PostChannelMessageAsync(string teamId, string channelId, string message, CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (token is null)
            return new { ok = false, message = "MS Graph not configured or token failed." };

        var client = httpClientFactory.CreateClient("MsGraph");
        var req = new HttpRequestMessage(HttpMethod.Post,
            $"https://graph.microsoft.com/v1.0/teams/{teamId}/channels/{channelId}/messages");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = new StringContent(
            JsonSerializer.Serialize(new { body = new { contentType = "html", content = message } }),
            System.Text.Encoding.UTF8, "application/json");
        var resp = await client.SendAsync(req, ct);
        logger.LogInformation("Teams message status {Status}", resp.StatusCode);
        return new { ok = resp.IsSuccessStatusCode, status = (int)resp.StatusCode };
    }
}
