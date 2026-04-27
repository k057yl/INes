using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Reminders.Commands.CompleteReminder
{
    public class CompleteReminderHandler : IRequestHandler<CompleteReminderCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public CompleteReminderHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(CompleteReminderCommand request, CancellationToken cancellationToken)
        {
            var reminder = await _context.Reminders
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.Id == request.ReminderId && r.Item.UserId == request.UserId, cancellationToken);

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
                Id = Guid.NewGuid(),
                ItemId = reminder.ItemId,
                Type = ItemHistoryType.ReminderCompleted,
                NewValue = $"{HISTORY.REMINDER.COMPLETED}|{reminder.Title}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}
