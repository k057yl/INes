using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Items.Commands.ChangeItemStatus
{
    public class ChangeItemStatusHandler : IRequestHandler<ChangeItemStatusCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public ChangeItemStatusHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(ChangeItemStatusCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            if (item.Status == request.NewStatus) return true;

            ItemHistoryType historyType;

            switch (request.NewStatus)
            {
                case ItemStatus.Active:
                    item.Return();
                    historyType = ItemHistoryType.Returned;
                    break;

                case ItemStatus.Lent:
                    item.Lend();
                    historyType = ItemHistoryType.Lent;
                    break;

                case ItemStatus.Sold:
                    item.Sell();
                    historyType = ItemHistoryType.Sold;
                    break;

                case ItemStatus.Archived:
                    item.Archive();
                    historyType = ItemHistoryType.Archived;
                    break;

                default:
                    throw new AppException(ITEMS.ERRORS.INVALID_INITIAL_STATUS);
            }

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = request.UserId,
                Type = historyType,
                OldValue = item.Status.ToString(),
                NewValue = request.NewStatus.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            _tracker.InvalidateUserCache(request.UserId);

            return true;
        }
    }
}