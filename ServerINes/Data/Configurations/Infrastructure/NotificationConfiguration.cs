using INest.Data.Entities.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Infrastructure
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.IsRead)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.UserId);
        }
    }
}
