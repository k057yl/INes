using INest.Data.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Core
{
    public class ItemDetailsConfiguration : IEntityTypeConfiguration<ItemDetails>
    {
        public void Configure(EntityTypeBuilder<ItemDetails> builder)
        {
            builder.HasKey(x => x.ItemId);

            builder.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            builder.Property(x => x.EstimatedValue).HasPrecision(18, 2);
            builder.Property(x => x.Currency).HasMaxLength(3);

            builder.Property(x => x.ReceiptDocumentPath).HasMaxLength(500);
        }
    }
}
