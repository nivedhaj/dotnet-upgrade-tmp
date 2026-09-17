using Ct.Core.Infrastructure;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ct.Modules.Admissions.Infrastructure.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.PersonId).HasColumnName("person_id");
        builder.Property(s => s.StudentId).HasColumnName("student_id").HasMaxLength(100);
        builder.Property(s => s.Attendance).HasColumnName("attendance").HasMaxLength(100);
        builder.Property(s => s.Category).HasColumnName("category").HasMaxLength(100);
        builder.Property(s => s.Status).HasColumnName("status").HasConversion<short>();
        builder.Property(s => s.AadharNumber).HasColumnName("aadhar_number").HasMaxLength(50);
        builder.Property(s => s.Udid).HasColumnName("udid").HasMaxLength(100);
        builder.Property(s => s.UserId).HasColumnName("user_id").HasMaxLength(200);
        builder.Property(s => s.PhotoUrl).HasColumnName("photo_url");
        builder.Property(s => s.Attributes).HasJsonbDictionary("attributes");
        builder.Property(s => s.CreatedOn).HasColumnName("created_on");
        builder.Property(s => s.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(s => s.ModifiedOn).HasColumnName("modified_on");
        builder.Property(s => s.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);

        builder.Ignore(s => s.OwnerId);
        builder.Ignore(s => s.ParentId);
        builder.Ignore(s => s.Audited);

        builder.HasOne(s => s.Person).WithMany().HasForeignKey(s => s.PersonId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Relations).WithOne().HasForeignKey(r => r.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Enrollments).WithOne().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Domicile).WithOne().HasForeignKey<StudentDomicile>(d => d.StudentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.StudentId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.UserId);
    }
}

public class StudentRelationConfiguration : IEntityTypeConfiguration<StudentRelation>
{
    public void Configure(EntityTypeBuilder<StudentRelation> builder)
    {
        builder.ToTable("student_relations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.StudentId).HasColumnName("student_id");
        builder.Property(r => r.PersonId).HasColumnName("person_id");
        builder.Property(r => r.RelationType).HasColumnName("relation_type").HasConversion<short>();

        builder.HasOne(r => r.Person).WithMany().HasForeignKey(r => r.PersonId).OnDelete(DeleteBehavior.Restrict);

        // Spec section 10: unique constraint on (StudentId, RelationType).
        builder.HasIndex(r => new { r.StudentId, r.RelationType }).IsUnique();
    }
}

public class StudentDomicileConfiguration : IEntityTypeConfiguration<StudentDomicile>
{
    public void Configure(EntityTypeBuilder<StudentDomicile> builder)
    {
        builder.ToTable("student_domiciles");
        builder.HasKey(d => d.StudentId);

        builder.Property(d => d.StudentId).HasColumnName("student_id");
        builder.Property(d => d.Caste).HasColumnName("caste").HasMaxLength(100);
        builder.Property(d => d.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(d => d.MotherTongue).HasColumnName("mother_tongue").HasMaxLength(100);
        builder.Property(d => d.State).HasColumnName("state").HasMaxLength(100);
    }
}

public class StudentEnrollmentConfiguration : IEntityTypeConfiguration<StudentEnrollment>
{
    public void Configure(EntityTypeBuilder<StudentEnrollment> builder)
    {
        builder.ToTable("student_enrollments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.StudentId).HasColumnName("student_id");
        builder.Property(e => e.InstId).HasColumnName("inst_id").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CourseId).HasColumnName("course_id").HasMaxLength(100);
        builder.Property(e => e.CourseName).HasColumnName("course_name").HasMaxLength(200);
        builder.Property(e => e.TermName).HasColumnName("term_name").HasMaxLength(100);
        builder.Property(e => e.SectionName).HasColumnName("section_name").HasMaxLength(100);
        builder.Property(e => e.Branch).HasColumnName("branch").HasMaxLength(100);
        builder.Property(e => e.StudentInstId).HasColumnName("student_inst_id").HasMaxLength(100);
        builder.Property(e => e.ApplicationNo).HasColumnName("application_no").HasMaxLength(100);
        builder.Property(e => e.AdmissionNo).HasColumnName("admission_no").HasMaxLength(100);
        builder.Property(e => e.Doa).HasColumnName("doa");
        builder.Property(e => e.RollNo).HasColumnName("roll_no").HasMaxLength(50);
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<short>();
        builder.Property(e => e.TcNo).HasColumnName("tc_no").HasMaxLength(100);
        builder.Property(e => e.TcAppliedOn).HasColumnName("tc_applied_on");
        builder.Property(e => e.TcIssuedOn).HasColumnName("tc_issued_on");
        builder.Property(e => e.Character).HasColumnName("character");
        builder.Property(e => e.Conduct).HasColumnName("conduct");
        builder.Property(e => e.Remarks).HasColumnName("remarks");
        builder.Property(e => e.Attributes).HasJsonbDictionary("attributes");

        // Spec section 61: unique (student_id, inst_id) — the legacy service
        // modeled one live enrollment per institute per student.
        builder.HasIndex(e => new { e.StudentId, e.InstId }).IsUnique();
        builder.HasIndex(e => e.InstId);
        builder.HasIndex(e => new { e.InstId, e.CourseId });
        builder.HasIndex(e => new { e.InstId, e.SectionName });
        builder.HasIndex(e => new { e.InstId, e.Status });
        builder.HasIndex(e => new { e.InstId, e.AdmissionNo });
    }
}
