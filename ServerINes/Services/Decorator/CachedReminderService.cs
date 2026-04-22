using INest.Models.DTOs.Reminder;
using INest.Models.Entities;
using INest.Services.Interfaces;
using INest.Services.Tracker;
using Microsoft.Extensions.Caching.Memory;

namespace INest.Services.Decorator
{
    public class CachedReminderService : IReminderService
    {
        private readonly IReminderService _inner;
        private readonly IMemoryCache _cache;
        private readonly ICacheTracker _tracker;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public CachedReminderService(IReminderService inner, IMemoryCache cache, ICacheTracker tracker)
        {
            _inner = inner;
            _cache = cache;
            _tracker = tracker;
        }

        public async Task<IEnumerable<Reminder>> GetActiveRemindersAsync(Guid userId)
        {
            var key = $"reminders_active_{userId}";
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AddExpirationToken(_tracker.GetToken(userId));
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _inner.GetActiveRemindersAsync(userId);
            }) ?? Enumerable.Empty<Reminder>();
        }

        public async Task<IEnumerable<Reminder>> GetItemRemindersAsync(Guid userId, Guid itemId)
        {
            var key = $"reminders_item_{userId}_{itemId}";
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AddExpirationToken(_tracker.GetToken(userId));
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _inner.GetItemRemindersAsync(userId, itemId);
            }) ?? Enumerable.Empty<Reminder>();
        }

        public async Task<Reminder> AddReminderAsync(Guid userId, CreateReminderDto dto)
        {
            var result = await _inner.AddReminderAsync(userId, dto);
            Invalidate(userId);
            return result;
        }

        public async Task<bool> CompleteReminderAsync(Guid userId, Guid reminderId)
        {
            var result = await _inner.CompleteReminderAsync(userId, reminderId);
            if (result) Invalidate(userId);
            return result;
        }

        public async Task<bool> DeleteReminderAsync(Guid userId, Guid reminderId)
        {
            var result = await _inner.DeleteReminderAsync(userId, reminderId);
            if (result) Invalidate(userId);
            return result;
        }

        private void Invalidate(Guid userId) => _tracker.InvalidateUserCache(userId);
    }
}