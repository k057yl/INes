using INest.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data
{
    public class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
    {
        public void Configure(EntityTypeBuilder<StorageLocation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            // self reference
            builder.HasOne(x => x.ParentLocation)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.StorageLocation)
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
