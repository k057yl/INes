using INest.Data.Enums;

namespace INest.Features.Items.DTOs
{
    public class ItemTreeDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ItemStatus Status { get; set; }

        public string? PhotoUrl { get; set; }
    }
}
