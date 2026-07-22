using INest.Data.Enums;
using INest.Features.Reminders.DTOs;

namespace INest.Features.Items.DTOs
{
    public class CreateItemDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid StorageLocationId { get; set; }
        public ItemStatus Status { get; set; } = ItemStatus.Active;

        public ItemFinanceDto Details { get; set; } = new();

        public CreateReminderDto? Reminder { get; set; }

        public string? PersonName { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public bool SendNotification { get; set; }
        public string? MainPhotoName { get; set; }
    }
}