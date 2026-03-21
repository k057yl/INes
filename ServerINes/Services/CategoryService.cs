using INest.Constants;
using INest.Exceptions;
using INest.Models.DTOs.Category;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category> UpdateAsync(Guid userId, Guid categoryId, CreateCategoryDto dto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

            if (category == null)
                throw new KeyNotFoundException(LocalizationConstants.CATEGORIES.NOT_FOUND);

            if (dto.ParentCategoryId == categoryId)
                throw new InvalidOperationException(LocalizationConstants.SYSTEM.VALIDATION_FAILED);

            category.Name = dto.Name;
            if (dto.Color != null) category.Color = dto.Color;
            category.ParentCategoryId = dto.ParentCategoryId;

            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteAsync(Guid userId, Guid categoryId, Guid? targetCategoryId = null)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

            if (category == null)
                throw new KeyNotFoundException(LocalizationConstants.CATEGORIES.NOT_FOUND);

            if (category.Name == SharedConstants.CATEGORY_OTHER)
                throw new InvalidOperationException(LocalizationConstants.CATEGORIES.CANNOT_DELETE_DEFAULT);

            if (category.Items.Any())
            {
                Guid targetId;

                if (targetCategoryId.HasValue && targetCategoryId.Value != categoryId)
                {
                    var targetExists = await _context.Categories.AnyAsync(c => c.Id == targetCategoryId.Value && c.UserId == userId);
                    targetId = targetExists ? targetCategoryId.Value : await GetOrCreateDefaultCategoryId(userId);
                }
                else
                {
                    targetId = await GetOrCreateDefaultCategoryId(userId);
                }

                foreach (var item in category.Items)
                {
                    item.CategoryId = targetId;
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        private async Task<Guid> GetOrCreateDefaultCategoryId(Guid userId)
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
                await _context.SaveChangesAsync();
            }

            return defaultCat.Id;
        }
    }
}
