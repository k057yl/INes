using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Features.Reminders.Services;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Reminders.Commands.CompleteReminder
{
    public class CompleteReminderHandler : IRequestHandler<CompleteReminderCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;
        private readonly IReminderScheduler _scheduler;

        public CompleteReminderHandler(AppDbContext context, ICacheTracker tracker, IReminderScheduler scheduler)
        {
            _context = context;
            _tracker = tracker;
            _scheduler = scheduler;
        }

        public async Task<bool> Handle(CompleteReminderCommand request, CancellationToken cancellationToken)
        {
            var reminder = await _context.Reminders
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.Id == request.ReminderId && r.Item.UserId == request.UserId, cancellationToken);

            if (reminder == null) return false;

            reminder.IsCompleted = true;

            var nextReminder = _scheduler.CreateNext(reminder, DateTime.UtcNow);
            if (nextReminder != null)
            {
                _context.Reminders.Add(nextReminder);
            }

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = reminder.ItemId,
                UserId = request.UserId,
                Type = ItemHistoryType.ReminderCompleted,
                NewValue = $"{HISTORY.REMINDER.COMPLETED}|{reminder.Title}"
            });

            await _context.SaveChangesAsync(cancellationToken);
            _tracker.InvalidateUserCache(request.UserId);

            return true;
        }
    }
}