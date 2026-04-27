using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Commands.ChangeItemStatus
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

            if (item.Status != request.NewStatus)
            {
                ItemHistoryType type = ItemHistoryType.StatusChanged;
                if (request.NewStatus == ItemStatus.Sold) type = ItemHistoryType.Sold;

                _context.ItemHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    Type = type,
                    OldValue = item.Status.ToString(),
                    NewValue = request.NewStatus.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                item.Status = request.NewStatus;
                await _context.SaveChangesAsync(cancellationToken);

                _tracker.InvalidateUserCache(request.UserId);
            }

            return true;
        }
    }
}