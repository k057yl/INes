using INest.Models.DTOs.Category;
using INest.Models.Entities;
using Microsoft.EntityFrameworkCore;
using INest.Services.Interfaces;
using INest.Constants;

namespace INest.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

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

        public async Task<Category?> UpdateAsync(Guid userId, Guid categoryId, CreateCategoryDto dto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

            if (category == null) return null;

            category.Name = dto.Name;
            if (dto.Color != null) category.Color = dto.Color;
            category.ParentCategoryId = dto.ParentCategoryId;

            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid categoryId, Guid? targetCategoryId = null)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

            if (category == null) return false;

            if (category.Name == SharedConstants.CATEGORY_OTHER)
                throw new InvalidOperationException("CannotDeleteDefaultCategory");

            if (category.Items.Any())
            {
                var targetId = targetCategoryId;
                if (targetId == null)
                {
                    var defaultCat = await _context.Categories
                        .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == SharedConstants.CATEGORY_OTHER);

                    if (defaultCat == null)
                    {
                        defaultCat = new Category
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Name = SharedConstants.CATEGORY_OTHER,
                            Color = "#64748b"
                        };
                        _context.Categories.Add(defaultCat);
                    }
                    targetId = defaultCat.Id;
                }

                foreach (var item in category.Items)
                {
                    item.CategoryId = targetId.Value;
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
