using INest.Models.DTOs.Category;
using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> CreateAsync(Guid userId, CreateCategoryDto dto);
        Task<IEnumerable<Category>> GetAllAsync(Guid userId);
    }
}
