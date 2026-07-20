using INest.Data.Entities.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INest.Data.Configurations.Finances
{
    public class LendingConfiguration : IEntityTypeConfiguration<Lending>
    {
        public void Configure(EntityTypeBuilder<Lending> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PersonName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.DateGiven)
                .IsRequired();

            builder.HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ItemId)
                .IsUnique();
        }
    }
}