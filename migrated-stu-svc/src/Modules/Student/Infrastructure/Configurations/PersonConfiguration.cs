using Ct.Core.Infrastructure;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ct.Modules.Admissions.Infrastructure.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.MiddleName).HasColumnName("middle_name").HasMaxLength(100);
        builder.Property(p => p.LastName).HasColumnName("last_name").HasMaxLength(100);
        builder.Property(p => p.Title).HasColumnName("title").HasMaxLength(50);
        builder.Property(p => p.Gender).HasColumnName("gender").HasConversion<short?>();
        builder.Property(p => p.Dob).HasColumnName("dob");
        builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(320);
        builder.Property(p => p.Mobile).HasColumnName("mobile").HasMaxLength(30);
        builder.Property(p => p.VirtualId).HasColumnName("virtual_id").HasMaxLength(320);
        builder.Property(p => p.Pan).HasColumnName("pan").HasMaxLength(20);
        builder.Property(p => p.Attributes).HasJsonbDictionary("attributes");
        builder.Property(p => p.CreatedOn).HasColumnName("created_on");
        builder.Property(p => p.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(p => p.ModifiedOn).HasColumnName("modified_on");
        builder.Property(p => p.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);

        // "Name" is a computed projection, not a stored column (spec section 7).
        builder.Ignore(p => p.Name);
        builder.Ignore(p => p.OwnerId);
        builder.Ignore(p => p.ParentId);
        builder.Ignore(p => p.Audited);

        builder.HasMany(p => p.Contacts).WithOne().HasForeignKey(c => c.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Addresses).WithOne().HasForeignKey(a => a.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Tags).WithOne().HasForeignKey(t => t.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Occupations).WithOne().HasForeignKey(o => o.PersonId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PersonContactConfiguration : IEntityTypeConfiguration<PersonContact>
{
    public void Configure(EntityTypeBuilder<PersonContact> builder)
    {
        builder.ToTable("person_contacts");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.PersonId).HasColumnName("person_id");
        builder.Property(c => c.ContactType).HasColumnName("contact_type").HasConversion<short>();
        builder.Property(c => c.Location).HasColumnName("location").HasConversion<short>();
        builder.Property(c => c.Info).HasColumnName("info").HasMaxLength(320);
        builder.Property(c => c.IsPrimary).HasColumnName("is_primary");
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<short>();
        builder.Property(c => c.Notes).HasColumnName("notes");
        builder.Property(c => c.ArchivedOn).HasColumnName("archived_on");
        builder.Property(c => c.CreatedOn).HasColumnName("created_on");

        builder.HasIndex(c => c.PersonId);
        builder.HasIndex(c => new { c.ContactType, c.Info });
        builder.HasIndex(c => new { c.PersonId, c.ContactType })
            .HasFilter("is_primary = true")
            .HasDatabaseName("ix_person_contacts_primary");
    }
}

public class PersonAddressConfiguration : IEntityTypeConfiguration<PersonAddress>
{
    public void Configure(EntityTypeBuilder<PersonAddress> builder)
    {
        builder.ToTable("person_addresses");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PersonId).HasColumnName("person_id");
        builder.Property(a => a.Location).HasColumnName("location").HasConversion<short>();
        builder.Property(a => a.Line1).HasColumnName("line1").HasMaxLength(200);
        builder.Property(a => a.Line2).HasColumnName("line2").HasMaxLength(200);
        builder.Property(a => a.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(a => a.State).HasColumnName("state").HasMaxLength(100);
        builder.Property(a => a.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(a => a.Pincode).HasColumnName("pincode").HasMaxLength(20);
        builder.Property(a => a.ArchivedOn).HasColumnName("archived_on");
        builder.Property(a => a.Status).HasColumnName("status").HasConversion<short>();

        builder.HasIndex(a => a.PersonId);
    }
}

public class PersonTagConfiguration : IEntityTypeConfiguration<PersonTag>
{
    public void Configure(EntityTypeBuilder<PersonTag> builder)
    {
        builder.ToTable("person_tags");
        builder.HasKey(t => new { t.PersonId, t.Tag });

        builder.Property(t => t.PersonId).HasColumnName("person_id");
        builder.Property(t => t.Tag).HasColumnName("tag").HasMaxLength(100);

        builder.HasIndex(t => t.Tag);
    }
}

public class PersonOccupationConfiguration : IEntityTypeConfiguration<PersonOccupation>
{
    public void Configure(EntityTypeBuilder<PersonOccupation> builder)
    {
        builder.ToTable("person_occupations");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.PersonId).HasColumnName("person_id");
        builder.Property(o => o.Title).HasColumnName("title").HasMaxLength(200);
        builder.Property(o => o.Particulars).HasColumnName("particulars");
        builder.Property(o => o.AsOn).HasColumnName("as_on");

        builder.HasIndex(o => o.PersonId);
    }
}
