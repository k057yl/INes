using INest.Constants;
using INest.Models.DTOs.Reminder;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services
{
    public class ReminderService : IReminderService
    {
        private readonly AppDbContext _context;
        public ReminderService(AppDbContext context) => _context = context;

        public async Task<Reminder> AddReminderAsync(Guid userId, CreateReminderDto dto)
        {
            var itemExists = await _context.Items.AnyAsync(i => i.Id == dto.ItemId && i.UserId == userId);
            if (!itemExists) throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                ItemId = dto.ItemId,
                Title = dto.Title,
                Type = dto.Type,
                Recurrence = dto.Recurrence,
                TriggerAt = dto.TriggerAt,
                IsCompleted = false,
                IsNotificationSent = false
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<bool> CompleteReminderAsync(Guid userId, Guid reminderId)
        {
            var reminder = await _context.Reminders
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.Id == reminderId && r.Item.UserId == userId);

            if (reminder == null) return false;

            reminder.IsCompleted = true;

            if (reminder.Recurrence != ReminderRecurrence.None)
            {
                DateTime nextTrigger = reminder.Recurrence switch
                {
                    ReminderRecurrence.Daily => reminder.TriggerAt.AddDays(1),
                    ReminderRecurrence.Weekly => reminder.TriggerAt.AddDays(7),
                    ReminderRecurrence.Monthly => reminder.TriggerAt.AddMonths(1),
                    ReminderRecurrence.Yearly => reminder.TriggerAt.AddYears(1),
                    _ => reminder.TriggerAt
                };

                _context.Reminders.Add(new Reminder
                {
                    Id = Guid.NewGuid(),
                    ItemId = reminder.ItemId,
                    Title = reminder.Title,
                    Type = reminder.Type,
                    Recurrence = reminder.Recurrence,
                    TriggerAt = nextTrigger,
                    IsCompleted = false
                });
            }

            _context.ItemHistories.Add(new ItemHistory
            {
                ItemId = reminder.ItemId,
                Type = ItemHistoryType.ReminderCompleted,
                NewValue = $"{LocalizationConstants.HISTORY.REMINDER_COMPLETED}|{reminder.Title}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReminderAsync(Guid userId, Guid reminderId)
        {
            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.Id == reminderId && r.Item.UserId == userId);

            if (reminder == null) return false;

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Reminder>> GetActiveRemindersAsync(Guid userId)
        {
            return await _context.Reminders
                .Where(r => r.Item.UserId == userId && !r.IsCompleted)
                .OrderBy(r => r.TriggerAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reminder>> GetItemRemindersAsync(Guid userId, Guid itemId)
        {
            return await _context.Reminders
                .Where(r => r.ItemId == itemId && r.Item.UserId == userId)
                .OrderByDescending(r => r.TriggerAt)
                .ToListAsync();
        }
    }
}
