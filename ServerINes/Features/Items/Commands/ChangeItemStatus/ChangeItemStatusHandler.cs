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

            var finalStatuses = new[] { ItemStatus.Sold, ItemStatus.Gifted, ItemStatus.Lost, ItemStatus.Broken };

            if (finalStatuses.Contains(item.Status))
            {
                throw new AppException(ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED);
            }

            if (item.Status == ItemStatus.Borrowed && request.NewStatus == ItemStatus.Active)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync(cancellationToken);
                _tracker.InvalidateUserCache(request.UserId);
                return true;
            }

            ItemHistoryType type = ItemHistoryType.StatusChanged;
            if (request.NewStatus == ItemStatus.Sold) type = ItemHistoryType.Sold;

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = request.UserId,
                Type = type,
                OldValue = item.Status.ToString(),
                NewValue = request.NewStatus.ToString()
            });

            item.Status = request.NewStatus;

            await _context.SaveChangesAsync(cancellationToken);
            _tracker.InvalidateUserCache(request.UserId);

            return true;
        }
    }
}