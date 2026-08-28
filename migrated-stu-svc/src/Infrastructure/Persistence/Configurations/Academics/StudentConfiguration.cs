using Ct.Modules.Academics.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ct.Infrastructure.Persistence.Configurations.Academics;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("student");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudentId).HasColumnName("student_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.Enrollments).HasColumnType("jsonb");
        builder.Property(x => x.Contacts).HasColumnType("jsonb");
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.HasIndex(x => x.StudentId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AdmissionNumber);
    }
}
