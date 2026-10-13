using System.Text.Json;
using Ct.Gr.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ct.Gr.Infrastructure;

public sealed class GrDbContext(DbContextOptions<GrDbContext> options) : DbContext(options)
{
    public DbSet<Receipt> Receipts => Set<Receipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonOpts = new JsonSerializerOptions();
        var receipt = modelBuilder.Entity<Receipt>();
        receipt.ToTable("receipts", "gr");
        receipt.HasKey(x => x.Id);
        receipt.Property(x => x.Number).HasMaxLength(64).IsRequired();
        receipt.Property(x => x.OwnerId).HasMaxLength(64).IsRequired();
        receipt.Property(x => x.InstId).HasMaxLength(64).IsRequired();
        receipt.HasIndex(x => new { x.OwnerId, x.Number }).IsUnique();
        receipt.HasIndex(x => new { x.OwnerId, x.Date });
        receipt.HasIndex(x => x.Status);

        receipt.OwnsOne(x => x.Customer, c =>
        {
            c.Property(p => p.Name).HasMaxLength(256);
            c.Property(p => p.Address).HasMaxLength(512);
            c.Property(p => p.Contact).HasMaxLength(64);
            c.Property(p => p.PAN).HasMaxLength(32);
            c.Property(p => p.StudyingIn).HasMaxLength(128);
            c.Property(p => p.Section).HasMaxLength(64);
        });
        receipt.OwnsOne(x => x.FinancialInstrument, f =>
        {
            f.Property(p => p.Number).HasMaxLength(32);
            f.Property(p => p.BankName).HasMaxLength(128);
            f.Property(p => p.BankBranch).HasMaxLength(128);
        });
        receipt.OwnsOne(x => x.Meta);

        var orderItemsConverter = new ValueConverter<List<OrderItem>, string>(
            v => JsonSerializer.Serialize(v, jsonOpts),
            v => JsonSerializer.Deserialize<List<OrderItem>>(v, jsonOpts) ?? new List<OrderItem>());
        var orderItemsComparer = new ValueComparer<List<OrderItem>>(
            (a, b) => JsonSerializer.Serialize(a, jsonOpts) == JsonSerializer.Serialize(b, jsonOpts),
            v => JsonSerializer.Serialize(v, jsonOpts).GetHashCode(),
            v => JsonSerializer.Deserialize<List<OrderItem>>(JsonSerializer.Serialize(v, jsonOpts), jsonOpts) ?? new());
        receipt.Property(x => x.OrderItems)
            .HasColumnType("jsonb")
            .HasConversion(orderItemsConverter)
            .Metadata.SetValueComparer(orderItemsComparer);

        base.OnModelCreating(modelBuilder);
    }
}
