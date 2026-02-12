namespace INest.Models.DTOs.Category
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;
        public string? Color { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }
}
