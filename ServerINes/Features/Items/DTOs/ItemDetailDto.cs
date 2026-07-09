using CategoryEntity = INest.Data.Entities.Core.Category;
using PhotoEntity = INest.Data.Entities.Core.ItemPhoto;
using LendingEntity = INest.Data.Entities.Finances.Lending;
using ReminderEntity = INest.Data.Entities.Infrastructure.Reminder;
using INest.Data.Entities.Core;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;

namespace INest.Features.Items.DTOs
{
    public class ItemDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ItemStatus Status { get; set; }

        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string Currency { get; set; } = "USD";
        public string? PhotoUrl { get; set; }

        public Guid? StorageLocationId { get; set; }
        public StorageLocation? StorageLocation { get; set; }

        public Guid CategoryId { get; set; }
        public CategoryEntity Category { get; set; } = null!;

        public bool IsLendingOverdue { get; set; }
        public bool HasOverdueReminders { get; set; }

        public ICollection<ItemHistory> History { get; set; } = new List<ItemHistory>();
        public ICollection<PhotoEntity> Photos { get; set; } = new List<PhotoEntity>();
        public ICollection<ReminderEntity> Reminders { get; set; } = new List<ReminderEntity>();

        public LendingEntity? Lending { get; set; }
    }
}