using INest.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SalePrice).HasPrecision(18, 2);
            builder.Property(x => x.Profit).HasPrecision(18, 2);
            builder.Property(x => x.ItemNameSnapshot).IsRequired().HasMaxLength(200);
            builder.Property(x => x.CategoryNameSnapshot).HasMaxLength(100);

            builder.HasOne(x => x.Item)
                .WithOne(x => x.Sale)
                .HasForeignKey<Sale>(x => x.ItemId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Platform)
                .WithMany()
                .HasForeignKey(x => x.PlatformId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
