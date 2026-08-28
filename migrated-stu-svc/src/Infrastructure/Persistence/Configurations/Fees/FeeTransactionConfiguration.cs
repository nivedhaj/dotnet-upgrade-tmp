using Ct.Modules.Fees.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ct.Infrastructure.Persistence.Configurations.Fees;

public sealed class FeeTransactionConfiguration : IEntityTypeConfiguration<FeeTransaction>
{
    public void Configure(EntityTypeBuilder<FeeTransaction> builder)
    {
        builder.ToTable("fee_transaction");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TxNo).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(14, 2);
        builder.Property(x => x.InstallmentsPaid).HasColumnName("installments_paid").HasColumnType("jsonb");
        builder.HasIndex(x => new { x.StudentId, x.TxDate });
        builder.HasIndex(x => x.TxNo).IsUnique();
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
    }
}
