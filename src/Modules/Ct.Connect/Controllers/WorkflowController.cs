using System.Text.Json;
using Ct.Connect.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Connect.Controllers;

/// <summary>Port of original WorkflowController (api/connect/Workflow).</summary>
[ApiController]
[Route("api/connect/workflow")]
[Route("api/connect/Workflow")]
[Route("api/Workflow")]
public sealed class WorkflowController(WorkflowService workflowService, Ct.Connect.Application.IMsTeamService teams) : ControllerBase
{
    [HttpGet("init")]
    public IActionResult Init([FromQuery] string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Empty title");
        var workflow = workflowService.For(title);
        if (workflow is null)
            return NotFound(new { message = $"Unknown workflow '{title}'" });
        return Ok(workflow);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] JsonElement body, CancellationToken ct)
    {
        await workflowService.PushAsync(body, ct);
        return Ok(body);
    }

    [HttpGet]
    public IActionResult Index()
        => Ok(new
        {
            endpoints = new[] { "GET init?title=email_generic|email_markscard|sms_generic", "POST /", "POST teams/message" },
            message = "Workflow init returns job graph; POST pushes payload to ct_r_workflow."
        });

    [HttpPost("teams/message")]
    public async Task<IActionResult> TeamsMessage([FromBody] TeamsMessageRequest request, CancellationToken ct)
    {
        var result = await teams.PostChannelMessageAsync(request.TeamId, request.ChannelId, request.Message, ct);
        return Ok(result);
    }
}

public sealed class TeamsMessageRequest
{
    public required string TeamId { get; init; }
    public required string ChannelId { get; init; }
    public required string Message { get; init; }
}

