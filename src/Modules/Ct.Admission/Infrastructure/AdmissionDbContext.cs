using System.Text.Json;
using Ct.Admission.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ct.Admission.Infrastructure;

public sealed class AdmissionDbContext(DbContextOptions<AdmissionDbContext> options) : DbContext(options)
{
    public DbSet<AdmissionApplication> Applications => Set<AdmissionApplication>();
    public DbSet<SeatMatrix> SeatMatrices => Set<SeatMatrix>();
    public DbSet<ApplicationFormTemplate> Templates => Set<ApplicationFormTemplate>();
    public DbSet<OtpRecord> Otps => Set<OtpRecord>();

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var a = modelBuilder.Entity<AdmissionApplication>();
        a.ToTable("adm_applications");
        a.HasKey(x => x.Id);
        a.HasIndex(x => x.OwnerId);
        a.HasIndex(x => new { x.OwnerId, x.Email });
        a.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        Jsonb(a, x => x.Identity); Jsonb(a, x => x.Father); Jsonb(a, x => x.Mother);
        Jsonb(a, x => x.Guardian); Jsonb(a, x => x.AcademicHistory); Jsonb(a, x => x.Documents);

        var s = modelBuilder.Entity<SeatMatrix>();
        s.ToTable("adm_seat_matrices");
        s.HasKey(x => x.Id);
        s.HasIndex(x => new { x.OwnerId, x.CourseId }).IsUnique();
        Jsonb(s, x => x.CategoryQuota);

        var t = modelBuilder.Entity<ApplicationFormTemplate>();
        t.ToTable("adm_form_templates");
        t.HasKey(x => x.Id);
        t.HasIndex(x => x.OwnerId);
        t.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        Jsonb(t, x => x.Fields); Jsonb(t, x => x.ShortlistCriteria);

        var o = modelBuilder.Entity<OtpRecord>();
        o.ToTable("adm_otps");
        o.HasKey(x => x.Id);
        o.HasIndex(x => x.MobileOrEmail);

        base.OnModelCreating(modelBuilder);
    }

    private static void Jsonb<TEntity, TProp>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> b,
        System.Linq.Expressions.Expression<Func<TEntity, TProp>> prop)
        where TEntity : class
        where TProp : class?, new()
    {
        b.Property(prop)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<TProp>(v, JsonOpts) ?? new TProp())
            .Metadata.SetValueComparer(new ValueComparer<TProp>(
                (c1, c2) => JsonSerializer.Serialize(c1, JsonOpts) == JsonSerializer.Serialize(c2, JsonOpts),
                v => JsonSerializer.Serialize(v, JsonOpts).GetHashCode(),
                v => JsonSerializer.Deserialize<TProp>(JsonSerializer.Serialize(v, JsonOpts), JsonOpts)!));
    }
}
