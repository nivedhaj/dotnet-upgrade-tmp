using Ct.Arte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Ct.Arte.Infrastructure.Persistence;

public sealed class ArtefactDbContext : DbContext
{
    public ArtefactDbContext(DbContextOptions<ArtefactDbContext> options) : base(options)
    {
    }

    public DbSet<Artefact> Artefacts => Set<Artefact>();
    public DbSet<ArtefactTag> ArtefactTags => Set<ArtefactTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // ---------- Artefact ----------
        modelBuilder.Entity<Artefact>(b =>
        {
            b.ToTable("artefacts");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasMaxLength(36).ValueGeneratedNever();

            b.Property(x => x.OwnerId).HasMaxLength(64).IsRequired();
            b.Property(x => x.ParentId).HasMaxLength(36);

            b.Property(x => x.Title).HasMaxLength(512).IsRequired();
            b.Property(x => x.Description);
            b.Property(x => x.Url).HasMaxLength(2048);
            b.Property(x => x.MimeType).HasMaxLength(128);
            b.Property(x => x.FileName).HasMaxLength(512);
            b.Property(x => x.FileSize);
            b.Property(x => x.Sha1).HasMaxLength(64);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            b.Property(x => x.Model).HasMaxLength(256);
            b.Property(x => x.Template);
            b.Property(x => x.Csv);
            b.Property(x => x.PublishedOn);

            b.Property(x => x.CreatedOn).IsRequired();
            b.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
            b.Property(x => x.ModifiedOn);
            b.Property(x => x.ModifiedBy).HasMaxLength(128);

            // JSONB columns
            b.Property(x => x.MetaData)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions) ?? new())
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToDictionary(x => x.Key, x => x.Value)));

            b.Property(x => x.Tags)
                .HasColumnType("text[]");

            b.Property(x => x.Comments)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<Comment>>(v, jsonOptions) ?? [])
                .Metadata.SetValueComparer(CreateListComparer<Comment>());

            b.Property(x => x.VideoLinks)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<VideoLink>>(v, jsonOptions) ?? [])
                .Metadata.SetValueComparer(CreateListComparer<VideoLink>());

            b.Property(x => x.DataAttributes)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<DataAttribute>>(v, jsonOptions) ?? [])
                .Metadata.SetValueComparer(CreateListComparer<DataAttribute>());

            b.Property(x => x.ChangeSet)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<ChangeRef>>(v, jsonOptions) ?? [])
                .Metadata.SetValueComparer(CreateListComparer<ChangeRef>());

            b.Property(x => x.PublicUrls).HasColumnType("text[]");
            b.Property(x => x.Thumbnails).HasColumnType("text[]");

            // Indexes
            b.HasIndex(x => x.OwnerId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.MimeType);
            b.HasIndex(x => x.CreatedOn);
            b.HasIndex(x => x.Tags).HasMethod("gin");
            b.HasIndex(x => x.MetaData).HasMethod("gin");
        });

        // ---------- ArtefactTag ----------
        modelBuilder.Entity<ArtefactTag>(b =>
        {
            b.ToTable("artefact_tags");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasMaxLength(36).ValueGeneratedNever();
            b.Property(x => x.OwnerId).HasMaxLength(64).IsRequired();
            b.Property(x => x.Name).HasMaxLength(256).IsRequired();
            b.Property(x => x.Csn).HasMaxLength(64).IsRequired();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

            b.Property(x => x.Meta)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions) ?? new());

            b.HasIndex(x => new { x.OwnerId, x.Name });
            b.HasIndex(x => x.Csn);
        });
    }

    private static ValueComparer<List<T>> CreateListComparer<T>() =>
        new(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v == null ? 0 : v.GetHashCode())),
            c => c.ToList());
}
