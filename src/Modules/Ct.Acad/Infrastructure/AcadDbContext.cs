using System.Text.Json;
using Ct.Acad.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ct.Acad.Infrastructure;

public sealed class AcadDbContext(DbContextOptions<AcadDbContext> options) : DbContext(options)
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentSubmission> AssessmentSubmissions => Set<AssessmentSubmission>();
    public DbSet<AttendanceEvent> AttendanceEvents => Set<AttendanceEvent>();
    public DbSet<GradingScheme> GradingSchemes => Set<GradingScheme>();
    public DbSet<AcadTag> Tags => Set<AcadTag>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<TimeTable> TimeTables => Set<TimeTable>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("acad");
        var json = new JsonSerializerOptions();

        void Jsonb<TEntity, TProp>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> e,
            System.Linq.Expressions.Expression<Func<TEntity, TProp>> prop) where TEntity : class where TProp : class, new()
        {
            var conv = new ValueConverter<TProp, string>(
                v => JsonSerializer.Serialize(v, json),
                v => JsonSerializer.Deserialize<TProp>(v, json) ?? new TProp());
            var cmp = new ValueComparer<TProp>(
                (a, b) => JsonSerializer.Serialize(a, json) == JsonSerializer.Serialize(b, json),
                v => JsonSerializer.Serialize(v, json).GetHashCode(),
                v => JsonSerializer.Deserialize<TProp>(JsonSerializer.Serialize(v, json), json) ?? new TProp());
            e.Property(prop).HasColumnType("jsonb").HasConversion(conv).Metadata.SetValueComparer(cmp);
        }

        var course = modelBuilder.Entity<Course>();
        course.ToTable("courses");
        course.HasKey(x => x.Id);
        course.HasIndex(x => new { x.OwnerId, x.Name, x.Branch });
        course.Property(x => x.Name).HasMaxLength(256).IsRequired();
        Jsonb(course, x => x.Terms);
        Jsonb(course, x => x.ExamSubjectOrder);

        var exam = modelBuilder.Entity<Exam>();
        exam.ToTable("exams");
        exam.HasKey(x => x.Id);
        exam.HasIndex(x => new { x.OwnerId, x.CourseId });
        exam.Property(x => x.Name).HasMaxLength(256).IsRequired();
        Jsonb(exam, x => x.Subjects);
        Jsonb(exam, x => x.Students);

        var assessment = modelBuilder.Entity<Assessment>();
        assessment.ToTable("assessments");
        assessment.HasKey(x => x.Id);
        Jsonb(assessment, x => x.QuestionIds);
        Jsonb(assessment, x => x.Tags);

        var sub = modelBuilder.Entity<AssessmentSubmission>();
        sub.ToTable("assessment_submissions");
        sub.HasKey(x => x.Id);
        sub.HasIndex(x => x.AssessmentId);
        Jsonb(sub, x => x.Answers);

        var att = modelBuilder.Entity<AttendanceEvent>();
        att.ToTable("attendance_events");
        att.HasKey(x => x.Id);
        att.HasIndex(x => new { x.OwnerId, x.Date });
        Jsonb(att, x => x.AbsenteeStudentIds);

        var grading = modelBuilder.Entity<GradingScheme>();
        grading.ToTable("grading_schemes");
        grading.HasKey(x => x.Id);
        Jsonb(grading, x => x.Rules);

        var tag = modelBuilder.Entity<AcadTag>();
        tag.ToTable("tags");
        tag.HasKey(x => x.Id);
        tag.HasIndex(x => new { x.OwnerId, x.Name }).IsUnique();

        var q = modelBuilder.Entity<Question>();
        q.ToTable("questions");
        q.HasKey(x => x.Id);
        q.HasIndex(x => x.OwnerId);
        Jsonb(q, x => x.Options);
        Jsonb(q, x => x.Tags);

        var tt = modelBuilder.Entity<TimeTable>();
        tt.ToTable("timetables");
        tt.HasKey(x => x.Id);
        Jsonb(tt, x => x.Periods);
        Jsonb(tt, x => x.Rules);

        var cal = modelBuilder.Entity<CalendarEvent>();
        cal.ToTable("calendar_events");
        cal.HasKey(x => x.Id);
        cal.HasIndex(x => new { x.OwnerId, x.Date });

        base.OnModelCreating(modelBuilder);
    }
}
