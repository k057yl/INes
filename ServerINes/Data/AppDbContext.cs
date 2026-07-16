using INest.Data.Entities;
using INest.Data.Entities.Core;
using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemPhoto> ItemPhotos => Set<ItemPhoto>();
    public DbSet<ItemHistory> ItemHistories => Set<ItemHistory>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Lending> Lendings => Set<Lending>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<TelegramConnectionCode> TelegramConnectionCodes { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

            foreach (var property in properties)
            {
                property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                ));
            }
        }
    }
}