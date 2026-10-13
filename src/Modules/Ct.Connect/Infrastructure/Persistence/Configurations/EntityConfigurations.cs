using Ct.Connect.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Ct.Connect.Infrastructure.Persistence.Configurations;

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("topics");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(36);
        builder.Property(e => e.OwnerId).HasMaxLength(36).IsRequired();
        builder.Property(e => e.ParentId).HasMaxLength(36);
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.FriendlyName).HasMaxLength(256);
        builder.Property(e => e.Description).HasMaxLength(2000);

        builder.Property(e => e.Meta)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.Default),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions.Default));

        builder.Property(e => e.Tags)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.Default),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<List<string>>(v, JsonOptions.Default));

        builder.Property(e => e.Subscriptions)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.Default),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, JsonOptions.Default));

        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => new { e.OwnerId, e.Status });
        builder.HasIndex(e => e.CreatedOn);
    }
}

public class SmsMessageConfiguration : IEntityTypeConfiguration<SmsMessage>
{
    public void Configure(EntityTypeBuilder<SmsMessage> builder)
    {
        builder.ToTable("sms_messages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(36);
        builder.Property(e => e.OwnerId).HasMaxLength(36).IsRequired();
        builder.Property(e => e.Message).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(1000);

        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => new { e.OwnerId, e.Status });
        builder.HasIndex(e => e.CreatedOn);
    }
}

public class SMSConfiguration : IEntityTypeConfiguration<SMS>
{
    public void Configure(EntityTypeBuilder<SMS> builder)
    {
        builder.ToTable("sms");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(36);
        builder.Property(e => e.OwnerId).HasMaxLength(36).IsRequired();
        builder.Property(e => e.Message).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.SMSRefId).HasMaxLength(128);
        builder.Property(e => e.GatewayResult).HasMaxLength(2000);

        builder.Property(e => e.Recipients)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.Default),
                v => string.IsNullOrEmpty(v)
                    ? new List<SMSRecipient>()
                    : JsonSerializer.Deserialize<List<SMSRecipient>>(v, JsonOptions.Default) ?? new List<SMSRecipient>());

        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => e.CreatedOn);
        // GIN index on recipients JSONB can be added via raw SQL migration
    }
}

public class EmailConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("emails");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(36);
        builder.Property(e => e.OwnerId).HasMaxLength(36).IsRequired();
        builder.Property(e => e.Type).HasMaxLength(64);
        builder.Property(e => e.From).HasMaxLength(320);
        builder.Property(e => e.Subject).HasMaxLength(998);
        builder.Property(e => e.Message).HasColumnType("text");

        builder.Property(e => e.Recipients)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.Default),
                v => string.IsNullOrEmpty(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, JsonOptions.Default) ?? new List<string>());

        builder.Property(e => e.Attachments)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.Default),
                v => string.IsNullOrEmpty(v)
                    ? null
                    : JsonSerializer.Deserialize<List<Attachment>>(v, JsonOptions.Default));

        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => new { e.OwnerId, e.Type });
        builder.HasIndex(e => e.CreatedOn);
    }
}
