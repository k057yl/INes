using INest.Data.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Core
{
    public class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
    {
        public void Configure(EntityTypeBuilder<StorageLocation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.UserId);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Color).HasMaxLength(50);
            builder.Property(x => x.Icon).HasMaxLength(50);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.StorageLocation)
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}