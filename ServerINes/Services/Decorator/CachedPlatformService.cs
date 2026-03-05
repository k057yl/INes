using INest.Constants;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace INest.Services.Decorator
{
    public class CachedPlatformService : IPlatformService
    {
        private readonly IPlatformService _inner;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public CachedPlatformService(IPlatformService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<IEnumerable<Platform>> GetAllAsync(Guid userId)
        {
            var key = CacheConstants.GetPlatformsKey(userId);
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _inner.GetAllAsync(userId);
            }) ?? Enumerable.Empty<Platform>();
        }

        public async Task<Platform> CreateAsync(Guid userId, string name)
        {
            var result = await _inner.CreateAsync(userId, name);
            _cache.Remove(CacheConstants.GetPlatformsKey(userId));
            return result;
        }

        public async Task<Platform?> UpdateAsync(Guid userId, Guid id, string name)
        {
            var result = await _inner.UpdateAsync(userId, id, name);
            if (result != null) _cache.Remove(CacheConstants.GetPlatformsKey(userId));
            return result;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid id)
        {
            var result = await _inner.DeleteAsync(userId, id);
            if (result) _cache.Remove(CacheConstants.GetPlatformsKey(userId));
            return result;
        }
    }
}
