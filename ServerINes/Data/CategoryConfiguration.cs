using INest.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.Categories)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ParentCategory)
                   .WithMany(x => x.Children)
                   .HasForeignKey(x => x.ParentCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
