using Ct.Library.Postgres.Domain;
using Marten.Events.Aggregation;

namespace Ct.Library.Postgres.ReadModels;

/// <summary>
/// Example domain events for Material (replace with real events from original codebase).
/// </summary>
public sealed class MaterialRegistered : DomainEvent
{
    public string AggregateId { get; set; } = string.Empty;
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string? Location { get; set; }
}

public sealed class MaterialTitleChanged : DomainEvent
{
    public string AggregateId { get; set; } = string.Empty;
    public string NewTitle { get; set; } = string.Empty;
}

public sealed class MaterialLocationChanged : DomainEvent
{
    public string AggregateId { get; set; } = string.Empty;
    public string NewLocation { get; set; } = string.Empty;
}

/// <summary>
/// Inline projection that builds / updates MaterialView from the event stream.
/// </summary>
public sealed class MaterialProjection : SingleStreamProjection<MaterialView, string>
{
    public MaterialProjection()
    {
        // Identity of the view document = Aggregate / stream Id
        Identity<MaterialRegistered>(e => e.AggregateId);
        Identity<MaterialTitleChanged>(e => e.AggregateId);
        Identity<MaterialLocationChanged>(e => e.AggregateId);
    }

    public MaterialView Create(MaterialRegistered e) => new()
    {
        Id = e.AggregateId,
        TrackingId = e.TrackingId,
        Title = e.Title,
        ISBN = e.ISBN,
        Author = e.Author,
        Publisher = e.Publisher,
        Location = e.Location,
        OwnerShip =
        [
            new Owner
            {
                OwnerId = e.OwnerId,
                Status = OwnerStatus.Current,
                From = e.OccurredOn
            }
        ],
        Status = "Available",
        CreatedOn = e.OccurredOn
    };

    public void Apply(MaterialTitleChanged e, MaterialView view)
    {
        view.Title = e.NewTitle;
        view.ModifiedOn = e.OccurredOn;
    }

    public void Apply(MaterialLocationChanged e, MaterialView view)
    {
        view.Location = e.NewLocation;
        view.ModifiedOn = e.OccurredOn;
    }
}
