using System.Text.Json;
using Ct.Staff.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ct.Staff.Infrastructure;

public sealed class StaffDbContext(DbContextOptions<StaffDbContext> options) : DbContext(options)
{
    public DbSet<StaffMember> Staff => Set<StaffMember>();
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var s = modelBuilder.Entity<StaffMember>();
        s.ToTable("staff");
        s.HasKey(x => x.Id);
        s.HasIndex(x => x.OwnerId);
        s.HasIndex(x => new { x.OwnerId, x.PrimaryEmail });
        s.Property(x => x.Name).HasMaxLength(256).IsRequired();
        s.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        s.Property(x => x.StaffType).HasConversion<string>().HasMaxLength(32);
        Jsonb(s, x => x.InstituteIds);
        Jsonb(s, x => x.Subjects);
        Jsonb(s, x => x.EmploymentHistory);
        Jsonb(s, x => x.Designations);
        Jsonb(s, x => x.Salaries);
        Jsonb(s, x => x.Timetable);
        Jsonb(s, x => x.Meta);

        var ss = modelBuilder.Entity<SalaryStructure>();
        ss.ToTable("salary_structures");
        ss.HasKey(x => x.Id);
        ss.HasIndex(x => x.OwnerId);
        Jsonb(ss, x => x.Earnings);
        Jsonb(ss, x => x.Deductions);

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
                (a, c) => JsonSerializer.Serialize(a, JsonOpts) == JsonSerializer.Serialize(c, JsonOpts),
                v => JsonSerializer.Serialize(v, JsonOpts).GetHashCode(),
                v => JsonSerializer.Deserialize<TProp>(JsonSerializer.Serialize(v, JsonOpts), JsonOpts)!));
    }
}
