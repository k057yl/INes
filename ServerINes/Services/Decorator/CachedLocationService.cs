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
        private const string CacheKeyPrefix = "locations_tree_";

        public CachedLocationService(ILocationService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        private string GetKey(Guid userId) => $"{CacheKeyPrefix}{userId}";

        public async Task<List<StorageLocation>> GetTreeAsync(Guid userId)
        {
            return await _cache.GetOrCreateAsync(GetKey(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await _inner.GetTreeAsync(userId);
            }) ?? new List<StorageLocation>();
        }

        public async Task<IEnumerable<object>> GetUserLocationsAsync(Guid userId)
        {
            string key = $"user_locations_list_{userId}";
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
            _cache.Remove(GetKey(userId));
            _cache.Remove($"user_locations_list_{userId}");
        }
    }
}