using INest.Data.Enums;
using INest.Features.Reminders.DTOs;

namespace INest.Features.Items.DTOs
{
    public class ItemTreeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ItemStatus Status { get; set; }

        public ItemFinanceDto? Details { get; set; }

        public string? PhotoUrl { get; set; }

        public List<ReminderDto> Reminders { get; set; } = new();
    }
}
