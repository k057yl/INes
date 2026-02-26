using INest.Models.DTOs.Category;
using INest.Models.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using INest.Services.Interfaces;

namespace INest.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public CategoryService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private string GetCacheKey(Guid userId) => $"categories_tree_{userId}";

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

            _cache.Remove(GetCacheKey(userId));

            return cat;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(Guid userId)
        {
            var key = GetCacheKey(userId);

            if (_cache.TryGetValue(key, out IEnumerable<Category>? cachedCategories))
            {
                return cachedCategories!;
            }

            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _cache.Set(key, categories, _cacheDuration);

            return categories;
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

            _cache.Remove(GetCacheKey(userId));

            return category;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid categoryId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

            if (category == null) return false;

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            _cache.Remove(GetCacheKey(userId));

            return true;
        }
    }
}
