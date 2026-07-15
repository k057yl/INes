using INest.Data;
using INest.Data.Entities.Core;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Items.Commands.MoveItem
{
    public class MoveItemHandler : IRequestHandler<MoveItemCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public MoveItemHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(MoveItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .Include(i => i.StorageLocation)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null) throw new AppException(ITEMS.ERRORS.NOT_FOUND, 404);

            if (item.StorageLocationId != request.TargetLocationId)
            {
                StorageLocation? targetLocation = null;

                if (request.TargetLocationId.HasValue)
                {
                    targetLocation = await _context.StorageLocations
                        .FirstOrDefaultAsync(l => l.Id == request.TargetLocationId.Value && l.UserId == request.UserId, cancellationToken);

                    if (targetLocation == null) throw new AppException(LOCATIONS.ERRORS.NOT_FOUND, 404);
                }

                _context.ItemHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    UserId = request.UserId,
                    Type = ItemHistoryType.Moved,
                    OldValue = item.StorageLocation?.Name,
                    NewValue = targetLocation?.Name,
                    CreatedAt = DateTime.UtcNow
                });

                item.MoveToLocation(request.TargetLocationId);

                await _context.SaveChangesAsync(cancellationToken);
                _tracker.InvalidateUserCache(request.UserId);
            }
            return true;
        }
    }
}