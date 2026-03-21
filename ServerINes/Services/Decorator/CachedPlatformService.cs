using INest.Constants;
using INest.Models.DTOs.Platform;
using INest.Models.Entities;
using INest.Services.Interfaces;
using INest.Services.Tracker;
using Microsoft.Extensions.Caching.Memory;

namespace INest.Services.Decorator
{
    public class CachedPlatformService : IPlatformService
    {
        private readonly IPlatformService _inner;
        private readonly IMemoryCache _cache;
        private readonly ICacheTracker _tracker;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public CachedPlatformService(IPlatformService inner, IMemoryCache cache, ICacheTracker tracker)
        {
            _inner = inner;
            _cache = cache;
            _tracker = tracker;
        }

        public async Task<IEnumerable<Platform>> GetAllAsync(Guid userId)
        {
            var key = CacheConstants.GET_PLATFORMS_KEY(userId);
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AddExpirationToken(_tracker.GetToken(userId));
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _inner.GetAllAsync(userId);
            }) ?? Enumerable.Empty<Platform>();
        }

        public async Task<Platform> CreateAsync(Guid userId, PlatformDto dto)
        {
            var result = await _inner.CreateAsync(userId, dto);
            Invalidate(userId);
            return result;
        }

        public async Task<Platform?> UpdateAsync(Guid userId, Guid id, PlatformDto dto)
        {
            var result = await _inner.UpdateAsync(userId, id, dto);
            if (result != null) Invalidate(userId);
            return result;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid id)
        {
            var result = await _inner.DeleteAsync(userId, id);
            if (result) Invalidate(userId);
            return result;
        }

        private void Invalidate(Guid userId) => _tracker.InvalidateUserCache(userId);
    }
}
