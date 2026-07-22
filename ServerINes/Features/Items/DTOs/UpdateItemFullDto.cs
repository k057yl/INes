using INest.Data.Enums;
using INest.Features.Reminders.DTOs;

namespace INest.Features.Items.DTOs
{
    public class UpdateItemFullDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public Guid StorageLocationId { get; set; }
        public ItemStatus Status { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string? Currency { get; set; }

        public DateTime? WarrantyExpiration { get; set; }

        public CreateReminderDto? Reminder { get; set; }

        public string? PersonName { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public bool SendNotification { get; set; }
        public List<IFormFile>? Photos { get; set; }
        public string? MainPhotoName { get; set; }
    }
}
