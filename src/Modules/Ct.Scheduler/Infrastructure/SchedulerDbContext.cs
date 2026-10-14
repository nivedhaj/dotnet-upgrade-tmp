using System.Text.Json;
using Ct.Scheduler.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ct.Scheduler.Infrastructure;

public sealed class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : DbContext(options)
{
    public DbSet<CalendarRule> Rules => Set<CalendarRule>();
    public DbSet<CalendarEvent> Events => Set<CalendarEvent>();

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var r = modelBuilder.Entity<CalendarRule>();
        r.ToTable("sched_rules");
        r.HasKey(x => x.Id);
        r.HasIndex(x => x.OwnerId);
        r.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        r.Property(x => x.Category).HasConversion<string>().HasMaxLength(32);
        Jsonb(r, x => x.WeeklyDays);

        var e = modelBuilder.Entity<CalendarEvent>();
        e.ToTable("sched_events");
        e.HasKey(x => x.Id);
        e.HasIndex(x => x.OwnerId);
        e.HasIndex(x => new { x.OwnerId, x.InstId });
        e.Property(x => x.Category).HasConversion<string>().HasMaxLength(32);
        Jsonb(e, x => x.Audience);
        Jsonb(e, x => x.ConductedBy);
        Jsonb(e, x => x.Occurrences);

        base.OnModelCreating(modelBuilder);
    }

    private static void Jsonb<TEntity, TProp>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> b,
        System.Linq.Expressions.Expression<Func<TEntity, TProp>> prop)
        where TEntity : class
        where TProp : class?, new()
    {
        b.Property(prop)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<TProp>(v, JsonOpts) ?? new TProp())
            .Metadata.SetValueComparer(new ValueComparer<TProp>(
                (a, c) => JsonSerializer.Serialize(a, JsonOpts) == JsonSerializer.Serialize(c, JsonOpts),
                v => JsonSerializer.Serialize(v, JsonOpts).GetHashCode(),
                v => JsonSerializer.Deserialize<TProp>(JsonSerializer.Serialize(v, JsonOpts), JsonOpts)!));
    }
}
