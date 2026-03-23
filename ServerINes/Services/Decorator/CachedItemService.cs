using INest.Constants;
using INest.Models.DTOs.Item;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using INest.Services.Tracker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace INest.Services.Decorator
{
    public class CachedItemService : IItemService
    {
        private readonly IItemService _inner;
        private readonly IMemoryCache _cache;
        private readonly ICacheTracker _tracker;

        public CachedItemService(IItemService inner, IMemoryCache cache, ICacheTracker tracker)
        {
            _inner = inner;
            _cache = cache;
            _tracker = tracker;
        }

        public async Task<IEnumerable<Item>> GetUserItemsAsync(Guid userId, ItemFilterDto filters)
        {
            string cacheKey = $"items_{userId}_{filters.SearchQuery}_{filters.CategoryId}_{filters.Status}_{filters.SortBy}_{filters.MinPrice}_{filters.MaxPrice}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AddExpirationToken(_tracker.GetToken(userId));
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetUserItemsAsync(userId, filters);
            }) ?? Enumerable.Empty<Item>();
        }

        private void InvalidateCache(Guid userId) => _tracker.InvalidateUserCache(userId);

        public async Task<Item> CreateItemAsync(Guid userId, CreateItemDto dto, List<IFormFile> photos)
        {
            var item = await _inner.CreateItemAsync(userId, dto, photos);
            InvalidateCache(userId);
            return item;
        }

        public async Task<bool> UpdateFullAsync(Guid userId, Guid itemId, UpdateItemFullDto dto, List<IFormFile>? photos)
        {
            var result = await _inner.UpdateFullAsync(userId, itemId, dto, photos);
            if (result) InvalidateCache(userId);
            return result;
        }

        public async Task<bool> UpdatePartialAsync(Guid userId, Guid itemId, UpdateItemPartialDto dto, List<IFormFile>? photos)
        {
            var result = await _inner.UpdatePartialAsync(userId, itemId, dto, photos);

            if (result)
                InvalidateCache(userId);

            return result;
        }

        public async Task<bool> MoveItemAsync(Guid userId, Guid itemId, Guid? targetLocationId)
        {
            var result = await _inner.MoveItemAsync(userId, itemId, targetLocationId);
            if (result) InvalidateCache(userId);
            return result;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid itemId)
        {
            var result = await _inner.DeleteAsync(userId, itemId);
            if (result) InvalidateCache(userId);
            return result;
        }

        public async Task<bool> ChangeStatusAsync(Guid userId, Guid itemId, ItemStatus newStatus)
        {
            var result = await _inner.ChangeStatusAsync(userId, itemId, newStatus);
            if (result) InvalidateCache(userId);
            return result;
        }

        public async Task<bool> CancelSaleAsync(Guid userId, Guid itemId)
        {
            var result = await _inner.CancelSaleAsync(userId, itemId);
            if (result) InvalidateCache(userId);
            return result;
        }

        public Task<Item?> GetItemAsync(Guid userId, Guid itemId) => _inner.GetItemAsync(userId, itemId);
        public Task<IEnumerable<ItemHistory>> GetItemHistoryAsync(Guid userId, Guid itemId) => _inner.GetItemHistoryAsync(userId, itemId);
    }
}
