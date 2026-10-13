using Ct.Connect.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ct.Connect.Infrastructure.Persistence;

public sealed class ConnectDbContext(DbContextOptions<ConnectDbContext> options) : DbContext(options)
{
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<SmsMessage> SmsMessages => Set<SmsMessage>();
    public DbSet<SMS> Sms => Set<SMS>();
    public DbSet<Email> Emails => Set<Email>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConnectDbContext).Assembly);

        // Global conventions
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // All string columns max length default (can be overridden)
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(string)))
            {
                if (property.GetMaxLength() is null)
                    property.SetMaxLength(512);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
