using INest.Models.DTOs.Category;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        public CategoryService(AppDbContext context) => _context = context;

        public async Task<Category> CreateAsync(Guid userId, CreateCategoryDto dto)
        {
            var cat = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Color = dto.Color ?? "#007bff",
                ParentCategoryId = dto.ParentCategoryId
            };

            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();
            return cat;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(Guid userId)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
    }
}
