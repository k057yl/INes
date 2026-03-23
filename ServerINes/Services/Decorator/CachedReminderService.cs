using INest.Models.DTOs.Reminder;
using INest.Models.Entities;
using INest.Services.Interfaces;
using INest.Services.Tracker;

namespace INest.Services.Decorator
{
    public class CachedReminderService : IReminderService
    {
        private readonly IReminderService _inner;
        private readonly ICacheTracker _tracker;

        public CachedReminderService(IReminderService inner, ICacheTracker tracker)
        {
            _inner = inner;
            _tracker = tracker;
        }

        public async Task<Reminder> AddReminderAsync(Guid userId, CreateReminderDto dto)
        {
            var result = await _inner.AddReminderAsync(userId, dto);
            _tracker.InvalidateUserCache(userId);
            return result;
        }

        public async Task<bool> CompleteReminderAsync(Guid userId, Guid reminderId)
        {
            var result = await _inner.CompleteReminderAsync(userId, reminderId);
            if (result) _tracker.InvalidateUserCache(userId);
            return result;
        }

        public async Task<bool> DeleteReminderAsync(Guid userId, Guid reminderId)
        {
            var result = await _inner.DeleteReminderAsync(userId, reminderId);
            if (result) _tracker.InvalidateUserCache(userId);
            return result;
        }

        public Task<IEnumerable<Reminder>> GetActiveRemindersAsync(Guid userId)
            => _inner.GetActiveRemindersAsync(userId);

        public Task<IEnumerable<Reminder>> GetItemRemindersAsync(Guid userId, Guid itemId)
            => _inner.GetItemRemindersAsync(userId, itemId);
    }
}
