using INest.Constants;
using INest.Models.DTOs.Location;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace INest.Services.Decorator
{
    public class CachedLocationService : ILocationService
    {
        private readonly ILocationService _inner;
        private readonly IMemoryCache _cache;

        public CachedLocationService(ILocationService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<List<StorageLocation>> GetTreeAsync(Guid userId)
        {
            var key = CacheConstants.GET_LOCATIONS_TREE_KEY(userId);
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await _inner.GetTreeAsync(userId);
            }) ?? new List<StorageLocation>();
        }

        public async Task<IEnumerable<object>> GetUserLocationsAsync(Guid userId)
        {
            var key = CacheConstants.GET_USER_LOCATIONS_LIST_KEY(userId);
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await _inner.GetUserLocationsAsync(userId);
            }) ?? Enumerable.Empty<object>();
        }

        public async Task<StorageLocation?> GetLocationByIdAsync(Guid userId, Guid locationId)
        {
            return await _inner.GetLocationByIdAsync(userId, locationId);
        }

        public async Task<StorageLocation> CreateLocationAsync(Guid userId, CreateLocationDto dto)
        {
            var result = await _inner.CreateLocationAsync(userId, dto);
            InvalidateCache(userId);
            return result;
        }

        public async Task RenameLocationAsync(Guid userId, Guid locationId, string newName)
        {
            await _inner.RenameLocationAsync(userId, locationId, newName);
            InvalidateCache(userId);
        }

        public async Task DeleteLocationAsync(Guid userId, Guid locationId)
        {
            await _inner.DeleteLocationAsync(userId, locationId);
            InvalidateCache(userId);
        }

        public async Task<bool> UpdateSortOrderAsync(Guid userId, Guid locationId, int newOrder)
        {
            var result = await _inner.UpdateSortOrderAsync(userId, locationId, newOrder);
            if (result) InvalidateCache(userId);
            return result;
        }

        public async Task MoveLocationAsync(Guid userId, Guid locationId, Guid? newParentId)
        {
            await _inner.MoveLocationAsync(userId, locationId, newParentId);
            InvalidateCache(userId);
        }

        public async Task ReorderLocationsAsync(Guid userId, Guid? parentId, List<Guid> orderedIds)
        {
            await _inner.ReorderLocationsAsync(userId, parentId, orderedIds);
            InvalidateCache(userId);
        }

        private void InvalidateCache(Guid userId)
        {
            _cache.Remove(CacheConstants.GET_LOCATIONS_TREE_KEY(userId));
            _cache.Remove(CacheConstants.GET_USER_LOCATIONS_LIST_KEY(userId));
        }
    }
}