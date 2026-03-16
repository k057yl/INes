using INest.Models.DTOs.Category;
using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync(Guid userId);
        Task<Category> CreateAsync(Guid userId, CreateCategoryDto dto);
        Task<Category> UpdateAsync(Guid userId, Guid categoryId, CreateCategoryDto dto);
        Task DeleteAsync(Guid userId, Guid categoryId, Guid? targetCategoryId = null);
    }
}
