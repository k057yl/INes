using INest.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace INest.Models.DTOs.Item
{
    public class CreateItemDto
    {
        // Основная инфа
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid StorageLocationId { get; set; }
        public ItemStatus Status { get; set; } = ItemStatus.Active;

        // Финансы и даты
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string? Currency { get; set; }

        // Одалживание
        public string? PersonName { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public bool SendNotification { get; set; }

        // Логика главного фото
        public string? MainPhotoName { get; set; }
    }
}