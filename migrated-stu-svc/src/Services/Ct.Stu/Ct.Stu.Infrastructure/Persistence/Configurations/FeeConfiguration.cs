using Ct.Stu.Domain; using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Ct.Stu.Infrastructure.Persistence.Configurations;
public sealed class FeeConfiguration : IEntityTypeConfiguration<Fee>{public void Configure(EntityTypeBuilder<Fee>b){b.ToTable("fee");b.HasKey(x=>x.Id);b.Property(x=>x.Name).HasMaxLength(250).IsRequired();b.Property(x=>x.Amount).HasPrecision(14,2);}}
