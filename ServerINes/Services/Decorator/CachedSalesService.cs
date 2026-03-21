using INest.Constants;
using INest.Models.DTOs.Sale;
using INest.Services.Interfaces;
using INest.Services.Tracker;
using Microsoft.Extensions.Caching.Memory;

namespace INest.Services.Decorator
{
    public class CachedSalesService : ISalesService
    {
        private readonly ISalesService _inner;
        private readonly IMemoryCache _cache;
        private readonly ICacheTracker _tracker;

        public CachedSalesService(ISalesService inner, IMemoryCache cache, ICacheTracker tracker)
        {
            _inner = inner;
            _cache = cache;
            _tracker = tracker;
        }

        public async Task<List<SaleResponseDto>> GetSalesAsync(Guid userId)
        {
            var key = CacheConstants.GET_SALES_HISTORY_KEY(userId);
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AddExpirationToken(_tracker.GetToken(userId));
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return await _inner.GetSalesAsync(userId);
            }) ?? new List<SaleResponseDto>();
        }

        public async Task<SaleResponseDto> SellItemAsync(Guid userId, SellItemRequestDto request)
        {
            var result = await _inner.SellItemAsync(userId, request);
            Invalidate(userId);
            return result;
        }

        public async Task<bool> CancelSaleAsync(Guid userId, Guid itemId)
        {
            var result = await _inner.CancelSaleAsync(userId, itemId);
            if (result) Invalidate(userId);
            return result;
        }

        public async Task<bool> DeleteSaleRecordAsync(Guid userId, Guid saleId)
        {
            var result = await _inner.DeleteSaleRecordAsync(userId, saleId);
            if (result) Invalidate(userId);
            return result;
        }

        public async Task<bool> SmartDeleteAsync(Guid userId, Guid saleId)
        {
            var result = await _inner.SmartDeleteAsync(userId, saleId);
            if (result) Invalidate(userId);
            return result;
        }

        private void Invalidate(Guid userId) => _tracker.InvalidateUserCache(userId);
    }
}
