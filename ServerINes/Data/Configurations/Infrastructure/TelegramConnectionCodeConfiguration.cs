using INest.Data.Entities.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Infrastructure
{
    public class TelegramConnectionCodeConfiguration : IEntityTypeConfiguration<TelegramConnectionCode>
    {
        public void Configure(EntityTypeBuilder<TelegramConnectionCode> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(x => x.ExpiryTime)
                .IsRequired();

            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.Code).IsUnique();
        }
    }
}