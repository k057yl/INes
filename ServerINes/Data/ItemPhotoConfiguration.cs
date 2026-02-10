using INest.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data
{
    public class ItemPhotoConfiguration : IEntityTypeConfiguration<ItemPhoto>
    {
        public void Configure(EntityTypeBuilder<ItemPhoto> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.IsMain)
                .IsRequired();

            builder.Property(x => x.UploadedAt)
                .IsRequired();

            builder.HasOne(x => x.Item)
                .WithMany(x => x.Photos)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}