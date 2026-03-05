using INest.Constants;
using INest.Models.DTOs.Category;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace INest.Services.Decorator
{
    public class CachedCategoryService : ICategoryService
    {
        private readonly ICategoryService _inner;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public CachedCategoryService(ICategoryService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(Guid userId)
        {
            var key = CacheConstants.GetCategoriesKey(userId);
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _inner.GetAllAsync(userId);
            }) ?? Enumerable.Empty<Category>();
        }

        public async Task<Category> CreateAsync(Guid userId, CreateCategoryDto dto)
        {
            var result = await _inner.CreateAsync(userId, dto);
            _cache.Remove(CacheConstants.GetCategoriesKey(userId));
            return result;
        }

        public async Task<Category?> UpdateAsync(Guid userId, Guid categoryId, CreateCategoryDto dto)
        {
            var result = await _inner.UpdateAsync(userId, categoryId, dto);
            if (result != null) _cache.Remove(CacheConstants.GetCategoriesKey(userId));
            return result;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid categoryId, Guid? targetCategoryId = null)
        {
            var result = await _inner.DeleteAsync(userId, categoryId, targetCategoryId);

            if (result)
            {
                _cache.Remove(CacheConstants.GetCategoriesKey(userId));
            }

            return result;
        }
    }
}
