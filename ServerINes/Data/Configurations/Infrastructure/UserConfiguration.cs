using INest.Data.Entities.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Infrastructure
{
    public class UserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.HasMany(x => x.Locations)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Notifications)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}