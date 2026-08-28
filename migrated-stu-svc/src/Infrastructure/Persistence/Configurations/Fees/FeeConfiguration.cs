using Ct.Modules.Fees.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ct.Infrastructure.Persistence.Configurations.Fees;

public sealed class FeeConfiguration : IEntityTypeConfiguration<Fee>
{
    public void Configure(EntityTypeBuilder<Fee> builder)
    {
        builder.ToTable("fee");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(14, 2);
    }
}
