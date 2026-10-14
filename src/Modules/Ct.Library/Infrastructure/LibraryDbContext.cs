using System.Text.Json;
using Ct.Library.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ct.Library.Infrastructure;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Circulation> Circulations => Set<Circulation>();

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var m = modelBuilder.Entity<Material>();
        m.ToTable("lib_materials");
        m.HasKey(x => x.Id);
        m.HasIndex(x => x.OwnerId);
        m.HasIndex(x => new { x.OwnerId, x.TrackingId }).IsUnique();
        m.HasIndex(x => x.ISBN);
        m.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        Jsonb(m, x => x.Attributes);
        Jsonb(m, x => x.Tags);

        var mem = modelBuilder.Entity<Member>();
        mem.ToTable("lib_members");
        mem.HasKey(x => x.Id);
        mem.HasIndex(x => x.OwnerId);
        mem.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        Jsonb(mem, x => x.Attributes);
        Jsonb(mem, x => x.Tags);

        var c = modelBuilder.Entity<Circulation>();
        c.ToTable("lib_circulations");
        c.HasKey(x => x.Id);
        c.HasIndex(x => x.OwnerId);
        c.HasIndex(x => x.MaterialId);
        c.HasIndex(x => x.MemberId);
        c.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

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
                (a, x) => JsonSerializer.Serialize(a, JsonOpts) == JsonSerializer.Serialize(x, JsonOpts),
                v => JsonSerializer.Serialize(v, JsonOpts).GetHashCode(),
                v => JsonSerializer.Deserialize<TProp>(JsonSerializer.Serialize(v, JsonOpts), JsonOpts)!));
    }
}
