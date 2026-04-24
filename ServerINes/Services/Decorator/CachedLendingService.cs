using INest.Models.DTOs.Lending;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using INest.Services.Tracker;

namespace INest.Services.Decorator
{
    public class CachedLendingService : ILendingService
    {
        private readonly ILendingService _inner;
        private readonly ICacheTracker _tracker;

        public CachedLendingService(ILendingService inner, ICacheTracker tracker)
        {
            _inner = inner;
            _tracker = tracker;
        }

        public async Task<Lending> LendItemAsync(Guid userId, LendItemDto dto)
        {
            var result = await _inner.LendItemAsync(userId, dto);
            _tracker.InvalidateUserCache(userId);
            return result;
        }

        public async Task<bool> ReturnItemAsync(Guid userId, Guid itemId, ReturnItemDto dto)
        {
            var result = await _inner.ReturnItemAsync(userId, itemId, dto);
            if (result) _tracker.InvalidateUserCache(userId);
            return result;
        }

        public Task SyncLendingStateAsync(Item item, ItemStatus newStatus, string personName, string? contactEmail, DateTime? expectedReturnDate, bool sendNotification)
        {
            return _inner.SyncLendingStateAsync(item, newStatus, personName, contactEmail, expectedReturnDate, sendNotification);
        }
    }
}
