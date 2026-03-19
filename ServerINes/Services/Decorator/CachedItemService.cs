using INest.Constants;
using INest.Models.DTOs.Item;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace INest.Services.Decorator
{
    public class CachedItemService : IItemService
    {
        private readonly IItemService _inner;
        private readonly IMemoryCache _cache;

        public CachedItemService(IItemService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<IEnumerable<Item>> GetUserItemsAsync(Guid userId, ItemFilterDto filters)
        {
            var key = CacheConstants.GET_ITEMS_KEY(userId);

            var allItems = await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetUserItemsAsync(userId, new ItemFilterDto());
            }) ?? Enumerable.Empty<Item>();

            var query = allItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
            {
                var search = filters.SearchQuery.ToLower();
                query = query.Where(i =>
                    i.Name.ToLower().Contains(search) ||
                    (i.Description != null && i.Description.ToLower().Contains(search)));
            }

            if (filters.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filters.CategoryId.Value);

            if (filters.Status.HasValue)
                query = query.Where(i => i.Status == filters.Status.Value);

            return query.ToList();
        }

        public async Task<Item> CreateItemAsync(Guid userId, CreateItemDto dto, List<IFormFile> photos)
        {
            var item = await _inner.CreateItemAsync(userId, dto, photos);
            InvalidateCache(userId);
            return item;
        }

        public async Task<bool> UpdateFullAsync(Guid userId, Guid itemId, UpdateItemFullDto dto, List<IFormFile>? photos)
        {
            var result = await _inner.UpdateFullAsync(userId, itemId, dto, photos);

            if (result)
                InvalidateCache(userId);

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

        public async Task<Item?> GetItemAsync(Guid userId, Guid itemId) =>
            await _inner.GetItemAsync(userId, itemId);

        public async Task<IEnumerable<ItemHistory>> GetItemHistoryAsync(Guid userId, Guid itemId) =>
            await _inner.GetItemHistoryAsync(userId, itemId);

        private void InvalidateCache(Guid userId)
        {
            _cache.Remove(CacheConstants.GET_ITEMS_KEY(userId));
            _cache.Remove(CacheConstants.GET_LOCATIONS_TREE_KEY(userId));
            _cache.Remove(CacheConstants.GET_USER_LOCATIONS_LIST_KEY(userId));
        }
    }
}
