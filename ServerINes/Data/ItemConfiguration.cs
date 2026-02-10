using INest.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            builder.Property(x => x.EstimatedValue).HasPrecision(18, 2);

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

            builder.HasOne(x => x.Lending)
                .WithOne(x => x.Item)
                .HasForeignKey<Lending>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
               .WithMany(x => x.Items)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
