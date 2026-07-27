using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Infrastructure
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<INest.Data.Entities.Infrastructure.Feedback>
    {
        public void Configure(EntityTypeBuilder<INest.Data.Entities.Infrastructure.Feedback> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.IsProcessed);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.Rating)
                .IsRequired(false);

            builder.Property(x => x.MissingFeatures)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsProcessed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
