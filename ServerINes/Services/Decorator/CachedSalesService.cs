using INest.Constants;
using INest.Models.DTOs.Sale;
using INest.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace INest.Services.Decorator
{
    public class CachedSalesService : ISalesService
    {
        private readonly ISalesService _inner;
        private readonly IMemoryCache _cache;

        public CachedSalesService(ISalesService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<List<SaleResponseDto>> GetSalesAsync(Guid userId)
        {
            var key = $"sales_history_{userId}";
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return await _inner.GetSalesAsync(userId);
            }) ?? new List<SaleResponseDto>();
        }

        public async Task<SaleResponseDto> SellItemAsync(Guid userId, SellItemRequestDto request)
        {
            var result = await _inner.SellItemAsync(userId, request);
            InvalidateCache(userId);
            return result;
        }

        public async Task<bool> CancelSaleAsync(Guid userId, Guid itemId)
        {
            var result = await _inner.CancelSaleAsync(userId, itemId);
            if (result) InvalidateCache(userId);
            return result;
        }

        public async Task<bool> DeleteSaleRecordAsync(Guid userId, Guid saleId)
        {
            var result = await _inner.DeleteSaleRecordAsync(userId, saleId);
            if (result) InvalidateCache(userId);
            return result;
        }

        public async Task<bool> SmartDeleteAsync(Guid userId, Guid saleId)
        {
            var result = await _inner.SmartDeleteAsync(userId, saleId);
            if (result) InvalidateCache(userId);
            return result;
        }

        private void InvalidateCache(Guid userId)
        {
            _cache.Remove($"sales_history_{userId}");
            _cache.Remove(CacheConstants.GetLocationsTreeKey(userId));
            _cache.Remove(CacheConstants.GetUserLocationsListKey(userId));
        }
    }
}
