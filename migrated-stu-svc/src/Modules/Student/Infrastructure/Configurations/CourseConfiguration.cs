using Ct.Core.Infrastructure;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ct.Modules.Admissions.Infrastructure.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.InstId).HasColumnName("inst_id").HasMaxLength(100);
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Branch).HasColumnName("branch").HasMaxLength(100);
        builder.Property(c => c.Affiliation).HasColumnName("affiliation").HasMaxLength(200);
        builder.Property(c => c.EduLevel).HasColumnName("edu_level").HasConversion<short?>();
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<short>();
        builder.Property(c => c.SortIndex).HasColumnName("sort_index");
        builder.Property(c => c.Rank).HasColumnName("rank");
        builder.Property(c => c.ExamSubjectOrder).HasJsonbList<string>("exam_subject_order");
        builder.Property(c => c.MarksCardTemplate).HasColumnName("marks_card_template");
        builder.Property(c => c.Attributes).HasJsonbDictionary("attributes");

        builder.HasMany(c => c.Terms).WithOne().HasForeignKey(t => t.CourseId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.InstId);
        builder.HasIndex(c => new { c.InstId, c.Status });
    }
}

public class CourseTermConfiguration : IEntityTypeConfiguration<CourseTerm>
{
    public void Configure(EntityTypeBuilder<CourseTerm> builder)
    {
        builder.ToTable("course_terms");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.CourseId).HasColumnName("course_id");
        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(100);
        builder.Property(t => t.DateFrom).HasColumnName("date_from");
        builder.Property(t => t.DateTo).HasColumnName("date_to");
        builder.Property(t => t.SortOrder).HasColumnName("sort_order");

        builder.HasMany(t => t.Sections).WithOne().HasForeignKey(s => s.TermId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
{
    public void Configure(EntityTypeBuilder<CourseSection> builder)
    {
        builder.ToTable("course_sections");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.TermId).HasColumnName("term_id");
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(100);
        builder.Property(s => s.SortOrder).HasColumnName("sort_order");

        builder.HasMany(s => s.Subjects).WithOne().HasForeignKey(sub => sub.SectionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CourseSubjectConfiguration : IEntityTypeConfiguration<CourseSubject>
{
    public void Configure(EntityTypeBuilder<CourseSubject> builder)
    {
        builder.ToTable("course_subjects");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.SectionId).HasColumnName("section_id");
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(s => s.Optional).HasColumnName("optional");
        builder.Property(s => s.SortOrder).HasColumnName("sort_order");
    }
}
