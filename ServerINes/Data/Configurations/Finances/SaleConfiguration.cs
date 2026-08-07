using INest.Data.Entities.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Finances
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.UserId);

            builder.Property(x => x.SalePrice).HasPrecision(18, 2);
            builder.Property(x => x.PurchasePriceSnapshot).HasPrecision(18, 2);
            builder.Property(x => x.PlatformFee).HasPrecision(18, 2);
            builder.Property(x => x.Profit).HasPrecision(18, 2);

            builder.Property(x => x.ItemNameSnapshot).IsRequired().HasMaxLength(200);
            builder.Property(x => x.CategoryNameSnapshot).HasMaxLength(100);
            builder.Property(x => x.Currency).HasMaxLength(10);

            builder.HasOne<Entities.Core.Item>()
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Platform)
                .WithMany()
                .HasForeignKey(x => x.PlatformId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}