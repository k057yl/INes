using INest.Models.Entities;
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

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}