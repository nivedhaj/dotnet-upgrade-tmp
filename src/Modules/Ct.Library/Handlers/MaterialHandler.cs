using Ct.Library.Postgres.Domain;
using Ct.Library.Postgres.Infrastructure;
using Ct.Library.Postgres.ReadModels;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Library.Postgres.Handlers;

/// <summary>
/// Example command handler for Material aggregate.
/// Demonstrates the write path through the new Marten EventStore.
/// </summary>
[ApiController]
[Route("api/materials")]
public sealed class MaterialHandler(
    IEventStore eventStore,
    ILogger<MaterialHandler> logger) : ControllerBase
{
    public sealed record RegisterMaterialCommand(
        string TrackingId,
        string Title,
        string? ISBN,
        string? Author,
        string? Publisher,
        string OwnerId,
        string? Location);

    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterMaterialCommand cmd,
        CancellationToken ct)
    {
        // In a real system you would load or create the aggregate properly.
        // Here we demonstrate the event-sourced write path.

        var aggregateId = Guid.NewGuid().ToString();

        // Create a transient aggregate instance (your real Material aggregate would live in Domain)
        var material = new MaterialAggregate(aggregateId);
        material.Register(
            cmd.TrackingId,
            cmd.Title,
            cmd.ISBN,
            cmd.Author,
            cmd.Publisher,
            cmd.OwnerId,
            cmd.Location);

        await eventStore.SaveAsync(material, ct);

        logger.LogInformation("Registered material {AggregateId} with tracking {TrackingId}",
            aggregateId, cmd.TrackingId);

        return CreatedAtAction(nameof(Register), new { id = aggregateId }, new { id = aggregateId });
    }
}

/// <summary>
/// Minimal Material aggregate for illustration.
/// Copy the real Material.cs from the original codebase and adapt event Apply methods.
/// </summary>
file sealed class MaterialAggregate : AggregateBase
{
    public string TrackingId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;

    public MaterialAggregate() { }

    public MaterialAggregate(string id)
    {
        Id = id;
    }

    public void Register(
        string trackingId,
        string title,
        string? isbn,
        string? author,
        string? publisher,
        string ownerId,
        string? location)
    {
        if (Version > 0)
            throw new InvalidOperationException("Material already registered");

        Apply(new MaterialRegistered
        {
            AggregateId = Id,
            TrackingId = trackingId,
            Title = title,
            ISBN = isbn,
            Author = author,
            Publisher = publisher,
            OwnerId = ownerId,
            Location = location
        });
    }

    // Apply methods are discovered by ConventionEventRouter
    private void Apply(MaterialRegistered e)
    {
        TrackingId = e.TrackingId;
        Title = e.Title;
    }
}
