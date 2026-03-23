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
                Type = dto.Type,
                TriggerAt = dto.TriggerAt,
                IsCompleted = false
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

            var recurringTypes = new[] {
            ReminderType.Insurance,
            ReminderType.Maintenance,
            ReminderType.MedicalCheckup,
            ReminderType.TaxPayment
        };

            if (recurringTypes.Contains(reminder.Type))
            {
                _context.Reminders.Add(new Reminder
                {
                    Id = Guid.NewGuid(),
                    ItemId = reminder.ItemId,
                    Type = reminder.Type,
                    TriggerAt = reminder.TriggerAt.AddYears(1),
                    IsCompleted = false
                });
            }

            _context.ItemHistories.Add(new ItemHistory
            {
                ItemId = reminder.ItemId,
                Type = ItemHistoryType.ValueUpdated,
                NewValue = $"{LocalizationConstants.HISTORY.REMINDER_COMPLETED}|{reminder.Type}",
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
