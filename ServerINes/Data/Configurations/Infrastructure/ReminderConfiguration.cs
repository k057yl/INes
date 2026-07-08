using INest.Data.Entities.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Infrastructure
{
    public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasQueryFilter(x => !x.IsDeleted);
            builder.HasIndex(x => new { x.UserId, x.IsDeleted });

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.TriggerAt)
                .IsRequired();

            builder.Property(x => x.IsCompleted)
                .IsRequired();

            builder.HasOne(x => x.Item)
                .WithMany(x => x.Reminders)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}