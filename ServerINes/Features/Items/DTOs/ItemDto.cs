using INest.Data.Entities.Core;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;

namespace INest.Features.Items.DTOs
{
    public class ItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ItemStatus Status { get; set; }

        public ItemFinanceDto? Details { get; set; }

        public string? PhotoUrl { get; set; }

        public Guid? StorageLocationId { get; set; }
        public string? StorageLocationName { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public bool IsOverdue { get; set; }
        public string? PersonName { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        public bool IsLendingOverdue { get; set; }
        public bool HasOverdueReminders { get; set; }

        public ICollection<ItemHistory> History { get; set; } = [];
        public ICollection<ItemPhoto> Photos { get; set; } = [];
        public ICollection<Reminder> Reminders { get; set; } = [];
    }
}
