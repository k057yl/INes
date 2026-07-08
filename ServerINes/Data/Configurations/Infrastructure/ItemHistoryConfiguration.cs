using INest.Data.Entities.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Infrastructure
{
    public class ItemHistoryConfiguration : IEntityTypeConfiguration<ItemHistory>
    {
        public void Configure(EntityTypeBuilder<ItemHistory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasIndex(x => new { x.UserId, x.IsDeleted });

            builder.Property(x => x.Type)
                .IsRequired();

            builder.HasOne(x => x.Item)
                .WithMany(x => x.History)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}