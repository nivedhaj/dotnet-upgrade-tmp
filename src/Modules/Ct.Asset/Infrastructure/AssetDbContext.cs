using System.Text.Json;
using Ct.Asset.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ct.Asset.Infrastructure;

public sealed class AssetDbContext(DbContextOptions<AssetDbContext> options) : DbContext(options)
{
    public DbSet<AssetItem> Assets => Set<AssetItem>();

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var a = modelBuilder.Entity<AssetItem>();
        a.ToTable("assets");
        a.HasKey(x => x.Id);
        a.HasIndex(x => x.OwnerId);
        a.HasIndex(x => new { x.OwnerId, x.TrackingId }).IsUnique();
        a.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        Jsonb(a, x => x.Attributes);
        Jsonb(a, x => x.Tags);
        Jsonb(a, x => x.Maintenance);
        Jsonb(a, x => x.CurrentWarranty);

        base.OnModelCreating(modelBuilder);
    }

    private static void Jsonb<TEntity, TProp>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> b,
        System.Linq.Expressions.Expression<Func<TEntity, TProp>> prop)
        where TEntity : class
    {
        b.Property(prop)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<TProp>(v, JsonOpts)!)
            .Metadata.SetValueComparer(new ValueComparer<TProp>(
                (c1, c2) => JsonSerializer.Serialize(c1, JsonOpts) == JsonSerializer.Serialize(c2, JsonOpts),
                v => v == null ? 0 : JsonSerializer.Serialize(v, JsonOpts).GetHashCode(),
                v => JsonSerializer.Deserialize<TProp>(JsonSerializer.Serialize(v, JsonOpts), JsonOpts)!));
    }
}
