using Ct.Connect.Application;
using Ct.Connect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Connect.Controllers;

[ApiController]
[Route("api/connect/topic")]
[Route("api/Topic")]
public sealed class TopicController(TopicCmd topics) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Topics([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await topics.GetTopicsAsync(activeOnly, ct));

    [HttpGet("mainTopics")]
    public async Task<IActionResult> GetMainTopics(CancellationToken ct)
        => Ok(await topics.GetMainTopicsAsync(ct));

    [HttpGet("handles")]
    public async Task<IActionResult> GetHandles(CancellationToken ct)
        => Ok(await topics.GetHandlesAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] List<CreateTopicRequest> requests, CancellationToken ct)
    {
        var created = await topics.CreateTopicsAsync(requests, ct);
        return Ok(created.Select(t => new { t.Id, t.Name }));
    }

    [HttpPost("mainTopic")]
    public async Task<IActionResult> CreateMainTopic([FromBody] CreateTopicRequest request, CancellationToken ct)
    {
        var t = await topics.CreateMainTopicAsync(request, ct);
        return Ok(new { t.Id, t.Name });
    }

    [HttpPost("updateMainTopic")]
    public async Task<IActionResult> UpdateMainTopic([FromBody] UpdateTopicFieldRequest request, CancellationToken ct)
    {
        await topics.UpdateMainTopicAsync(request.Id, request.Value, ct);
        return Ok();
    }

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] IdRequest request, CancellationToken ct)
    {
        await topics.EnableAsync(request.Id, ct);
        return Ok();
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] IdRequest request, CancellationToken ct)
    {
        await topics.DisableAsync(request.Id, ct);
        return Ok();
    }

    [HttpPost("updateFriendlyName")]
    public async Task<IActionResult> UpdateFriendlyName([FromBody] UpdateTopicFieldRequest request, CancellationToken ct)
    {
        await topics.UpdateFriendlyNameAsync(request.Id, request.Value, ct);
        return Ok();
    }

    [HttpPost("updateDescription")]
    public async Task<IActionResult> UpdateDescription([FromBody] UpdateTopicFieldRequest request, CancellationToken ct)
    {
        await topics.UpdateDescriptionAsync(request.Id, request.Value, ct);
        return Ok();
    }

    [HttpPost("updateCategory")]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateTopicFieldRequest request, CancellationToken ct)
    {
        await topics.UpdateCategoryAsync(request.Id, request.Value ?? "", ct);
        return Ok();
    }

    [HttpPost("updateRole")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateTopicFieldRequest request, CancellationToken ct)
    {
        await topics.UpdateRoleAsync(request.Id, request.Value ?? "", ct);
        return Ok();
    }

    [HttpPost("updateAccess")]
    public async Task<IActionResult> UpdateAccess([FromBody] UpdateTopicFieldRequest request, CancellationToken ct)
    {
        await topics.UpdateAccessAsync(request.Id, request.Value ?? "", ct);
        return Ok();
    }

    [HttpPost("updateSubscriptions")]
    public async Task<IActionResult> UpdateSubscriptions([FromBody] UpdateSubscriptionsRequest request, CancellationToken ct)
    {
        await topics.UpdateSubscriptionsAsync(request.Id, request.Subscriptions, ct);
        return Ok();
    }

    [HttpPost("subscriptionAllowed")]
    public async Task<IActionResult> SubscriptionAllowed(
        [FromBody] SubscriptionAllowedRequest? body,
        [FromQuery] string? id,
        [FromQuery] bool? allowed,
        CancellationToken ct)
    {
        var topicId = body?.Id ?? id ?? throw new ArgumentException("Id required");
        var flag = body?.IsSubscriptionAllowed ?? allowed ?? true;
        await topics.SetSubscriptionAllowedAsync(topicId, flag, ct);
        return Ok(new { message = "Topic updated" });
    }

    [HttpPost("handle")]
    public async Task<IActionResult> UpdateHandle([FromBody] HandleRequest request, CancellationToken ct)
    {
        try
        {
            await topics.UpdateHandleAsync(request.Id, request.Handle, ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status409Conflict, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
