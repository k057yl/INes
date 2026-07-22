using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Lendings.Commands.ReturnItem
{
    public class ReturnItemHandler : IRequestHandler<ReturnItemCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public ReturnItemHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(ReturnItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            var lending = await _context.Lendings
                .FirstOrDefaultAsync(l => l.ItemId == item.Id, cancellationToken);

            if (lending == null)
                throw new KeyNotFoundException(LENDING.ERRORS.NOT_LENT);

            bool isBorrowed = item.Status == ItemStatus.Borrowed;

            if (isBorrowed)
            {
                item.ReturnBorrowed();
            }
            else
            {
                item.Return();
            }

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = request.UserId,
                Type = ItemHistoryType.Returned,
                NewValue = isBorrowed ? HISTORY.RETURNED_BORROWED : HISTORY.RETURNED,
                CreatedAt = DateTime.UtcNow
            });

            var returnReminders = await _context.Reminders
                .Where(r => r.ItemId == item.Id && r.Type == ReminderType.ReturnItem)
                .ToListAsync(cancellationToken);

            if (returnReminders.Any())
            {
                _context.Reminders.RemoveRange(returnReminders);
            }

            _context.Lendings.Remove(lending);

            if (isBorrowed)
            {
                _context.Items.Remove(item);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}