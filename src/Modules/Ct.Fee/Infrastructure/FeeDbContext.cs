using Ct.Fee.Application;
using Ct.Fee.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ct.Fee.Infrastructure;

public sealed class FeeDbContext(DbContextOptions<FeeDbContext> options, ICurrentUserContext currentUser) : DbContext(options)
{
    public Guid CurrentOwnerId => currentUser.OwnerId;

    public DbSet<FeeDefinition> Fees => Set<FeeDefinition>();
    public DbSet<FeeTransaction> FeeTransactions => Set<FeeTransaction>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<SuspenseAccountTransaction> SuspenseAccountTransactions => Set<SuspenseAccountTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("fee");
        var stringArrayComparer = new ValueComparer<string[]>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            values => values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value.GetHashCode(StringComparison.Ordinal))),
            values => values.ToArray());
        var tagsConverter = new ValueConverter<string[], string>(
            values => string.Join('|', values),
            value => string.IsNullOrWhiteSpace(value) ? Array.Empty<string>() : value.Split('|', StringSplitOptions.RemoveEmptyEntries));

        ConfigureAudited<FeeDefinition>(modelBuilder.Entity<FeeDefinition>());
        modelBuilder.Entity<FeeDefinition>(entity =>
        {
            entity.ToTable("fee");
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NameLower).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DisplayText).HasMaxLength(300);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Tags).HasConversion(tagsConverter).Metadata.SetValueComparer(stringArrayComparer);
            entity.HasIndex(x => new { x.OwnerId, x.NameLower }).IsUnique();
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeInstallment>(modelBuilder.Entity<FeeInstallment>());
        modelBuilder.Entity<FeeInstallment>(entity =>
        {
            entity.ToTable("fee_installment");
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.FeeDefinitionId, x.InstallmentNo }).IsUnique();
            entity.HasOne(x => x.FeeDefinition).WithMany(x => x.Installments).HasForeignKey(x => x.FeeDefinitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeFine>(modelBuilder.Entity<FeeFine>());
        modelBuilder.Entity<FeeFine>(entity =>
        {
            entity.ToTable("fee_fine");
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.FeeDefinitionId, x.FineNo }).IsUnique();
            entity.HasOne(x => x.FeeDefinition).WithMany(x => x.Fines).HasForeignKey(x => x.FeeDefinitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeStudent>(modelBuilder.Entity<FeeStudent>());
        modelBuilder.Entity<FeeStudent>(entity =>
        {
            entity.ToTable("fee_student");
            entity.Property(x => x.StudentId).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.FeeDefinitionId, x.StudentId }).IsUnique();
            entity.HasOne(x => x.FeeDefinition).WithMany(x => x.Students).HasForeignKey(x => x.FeeDefinitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeCourse>(modelBuilder.Entity<FeeCourse>());
        modelBuilder.Entity<FeeCourse>(entity =>
        {
            entity.ToTable("fee_course");
            entity.Property(x => x.CourseId).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.FeeDefinitionId, x.CourseId }).IsUnique();
            entity.HasOne(x => x.FeeDefinition).WithMany(x => x.Courses).HasForeignKey(x => x.FeeDefinitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeTransaction>(modelBuilder.Entity<FeeTransaction>());
        modelBuilder.Entity<FeeTransaction>(entity =>
        {
            entity.ToTable("fee_transaction");
            entity.Property(x => x.TxNo).HasMaxLength(100).IsRequired();
            entity.Property(x => x.StudentId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PaymentMode).HasMaxLength(100);
            entity.Property(x => x.RefNo).HasMaxLength(150);
            entity.Property(x => x.ReceivedInAccount).HasMaxLength(150);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.OwnerId, x.TxNo }).IsUnique();
            entity.HasIndex(x => new { x.OwnerId, x.StudentId, x.TxDate });
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeTransactionInstallment>(modelBuilder.Entity<FeeTransactionInstallment>());
        modelBuilder.Entity<FeeTransactionInstallment>(entity =>
        {
            entity.ToTable("fee_transaction_installment");
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.FeeTransaction).WithMany(x => x.InstallmentsPaid).HasForeignKey(x => x.FeeTransactionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeTransactionFine>(modelBuilder.Entity<FeeTransactionFine>());
        modelBuilder.Entity<FeeTransactionFine>(entity =>
        {
            entity.ToTable("fee_transaction_fine");
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.FeeTransaction).WithMany(x => x.FinesPaid).HasForeignKey(x => x.FeeTransactionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeTransactionDiscount>(modelBuilder.Entity<FeeTransactionDiscount>());
        modelBuilder.Entity<FeeTransactionDiscount>(entity =>
        {
            entity.ToTable("fee_transaction_discount");
            entity.Property(x => x.Reason).HasMaxLength(300);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.FeeTransaction).WithMany(x => x.Discounts).HasForeignKey(x => x.FeeTransactionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<FeeTransactionAdjustment>(modelBuilder.Entity<FeeTransactionAdjustment>());
        modelBuilder.Entity<FeeTransactionAdjustment>(entity =>
        {
            entity.ToTable("fee_transaction_adjustment");
            entity.Property(x => x.Reason).HasMaxLength(300);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.FeeTransaction).WithMany(x => x.Adjustments).HasForeignKey(x => x.FeeTransactionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<Refund>(modelBuilder.Entity<Refund>());
        modelBuilder.Entity<Refund>(entity =>
        {
            entity.ToTable("refund");
            entity.Property(x => x.StudentId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.Property(x => x.Reference).HasMaxLength(150);
            entity.HasOne(x => x.FeeTransaction).WithMany().HasForeignKey(x => x.FeeTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.OwnerId, x.StudentId, x.RefundDate });
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<SuspenseAccountTransaction>(modelBuilder.Entity<SuspenseAccountTransaction>());
        modelBuilder.Entity<SuspenseAccountTransaction>(entity =>
        {
            entity.ToTable("suspense_account_transaction");
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.PaymentMode).HasMaxLength(100);
            entity.Property(x => x.Particulars).HasMaxLength(500);
            entity.HasIndex(x => new { x.OwnerId, x.TransactionDate, x.Status });
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });

        ConfigureAudited<SuspenseAccountTransactionAllocation>(modelBuilder.Entity<SuspenseAccountTransactionAllocation>());
        modelBuilder.Entity<SuspenseAccountTransactionAllocation>(entity =>
        {
            entity.ToTable("suspense_account_transaction_allocation");
            entity.Property(x => x.AllocatedAmount).HasPrecision(18, 2);
            entity.Property(x => x.AllocatedBy).HasMaxLength(150);
            entity.HasOne(x => x.SuspenseAccountTransaction).WithMany(x => x.Allocations).HasForeignKey(x => x.SuspenseAccountTransactionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.FeeTransaction).WithMany().HasForeignKey(x => x.FeeTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => CurrentOwnerId == Guid.Empty || x.OwnerId == CurrentOwnerId);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    private static void ConfigureAudited<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity) where TEntity : AuditableEntity
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CreatedBy).HasMaxLength(150);
        entity.Property(x => x.ModifiedBy).HasMaxLength(150);
        entity.HasIndex(x => x.OwnerId);
    }

    private void ApplyAudit()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.OwnerId = entry.Entity.OwnerId == Guid.Empty ? currentUser.OwnerId : entry.Entity.OwnerId;
                entry.Entity.ParentId ??= currentUser.ParentId;
                entry.Entity.CreatedOn = now;
                entry.Entity.CreatedBy = currentUser.UserId;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.ModifiedOn = now;
                entry.Entity.ModifiedBy = currentUser.UserId;
            }
        }
    }
}
