using System.Text.Json;
using Ct.Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ct.Accounting.Infrastructure;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options) : DbContext(options)
{
    public DbSet<LedgerAccount> Ledgers => Set<LedgerAccount>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryJournal> InventoryJournals => Set<InventoryJournal>();
    public DbSet<SmsCreditTx> SmsCreditTxs => Set<SmsCreditTx>();

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var l = modelBuilder.Entity<LedgerAccount>();
        l.ToTable("acc_ledgers");
        l.HasKey(x => x.Id);
        l.HasIndex(x => x.OwnerId);
        l.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        l.Property(x => x.Nature).HasConversion<string>().HasMaxLength(32);
        l.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

        var v = modelBuilder.Entity<Voucher>();
        v.ToTable("acc_vouchers");
        v.HasKey(x => x.Id);
        v.HasIndex(x => x.OwnerId);
        v.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        v.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        v.Property(x => x.Lines)
            .HasColumnType("jsonb")
            .HasConversion(
                x => JsonSerializer.Serialize(x, JsonOpts),
                x => JsonSerializer.Deserialize<List<VoucherLine>>(x, JsonOpts) ?? new())
            .Metadata.SetValueComparer(new ValueComparer<List<VoucherLine>>(
                (a, b) => JsonSerializer.Serialize(a, JsonOpts) == JsonSerializer.Serialize(b, JsonOpts),
                x => JsonSerializer.Serialize(x, JsonOpts).GetHashCode(),
                x => JsonSerializer.Deserialize<List<VoucherLine>>(JsonSerializer.Serialize(x, JsonOpts), JsonOpts)!));

        var i = modelBuilder.Entity<InventoryItem>();
        i.ToTable("acc_inventory_items");
        i.HasKey(x => x.Id);
        i.HasIndex(x => new { x.OwnerId, x.Sku });

        var j = modelBuilder.Entity<InventoryJournal>();
        j.ToTable("acc_inventory_journals");
        j.HasKey(x => x.Id);
        j.HasIndex(x => x.OwnerId);
        j.Property(x => x.TxType).HasConversion<string>().HasMaxLength(32);

        var s = modelBuilder.Entity<SmsCreditTx>();
        s.ToTable("acc_sms_credit_txs");
        s.HasKey(x => x.Id);
        s.HasIndex(x => x.OwnerId);

        base.OnModelCreating(modelBuilder);
    }
}
