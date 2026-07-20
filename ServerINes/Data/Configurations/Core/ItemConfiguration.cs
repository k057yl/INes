using INest.Data.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Core
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.UserId, x.Status });

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(x => x.Details)
                .WithOne(x => x.Item)
                .HasForeignKey<ItemDetails>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Photos)
                .WithOne(x => x.Item)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.History)
                .WithOne(x => x.Item)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Reminders)
                .WithOne(x => x.Item)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
               .WithMany(x => x.Items)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.StorageLocation)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}