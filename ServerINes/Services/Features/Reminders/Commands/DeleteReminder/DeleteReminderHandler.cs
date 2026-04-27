using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Reminders.Commands.DeleteReminder
{
    public class DeleteReminderHandler : IRequestHandler<DeleteReminderCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public DeleteReminderHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
        {
            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.Id == request.ReminderId && r.Item.UserId == request.UserId, cancellationToken);

            if (reminder == null) return false;

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}
