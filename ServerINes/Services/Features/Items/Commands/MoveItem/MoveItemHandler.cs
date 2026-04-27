using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Commands.MoveItem
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

            if (item == null) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            if (item.StorageLocationId != request.TargetLocationId)
            {
                string? oldLocName = item.StorageLocation?.Name;
                string? newLocName = null;

                if (request.TargetLocationId.HasValue)
                {
                    var targetLoc = await _context.StorageLocations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(l => l.Id == request.TargetLocationId.Value, cancellationToken);
                    newLocName = targetLoc?.Name;
                }

                _context.ItemHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    Type = ItemHistoryType.Moved,
                    OldValue = oldLocName,
                    NewValue = newLocName,
                    CreatedAt = DateTime.UtcNow
                });

                var oldStatus = item.Status;

                if (request.TargetLocationId.HasValue)
                {
                    var targetLocation = await _context.StorageLocations
                        .FirstOrDefaultAsync(l => l.Id == request.TargetLocationId.Value, cancellationToken);

                    if (targetLocation != null)
                    {
                        if (targetLocation.IsSalesLocation) item.Status = ItemStatus.Listed;
                        else if (targetLocation.IsLendingLocation) item.Status = ItemStatus.Lent;
                        else if (item.Status == ItemStatus.Listed || item.Status == ItemStatus.Lent)
                            item.Status = ItemStatus.Active;
                    }
                }
                else if (item.Status == ItemStatus.Listed || item.Status == ItemStatus.Lent)
                {
                    item.Status = ItemStatus.Active;
                }

                if (oldStatus != item.Status)
                {
                    _context.ItemHistories.Add(new ItemHistory
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        Type = ItemHistoryType.StatusChanged,
                        OldValue = oldStatus.ToString(),
                        NewValue = item.Status.ToString(),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                item.StorageLocationId = request.TargetLocationId;
                await _context.SaveChangesAsync(cancellationToken);

                _tracker.InvalidateUserCache(request.UserId);
            }

            return true;
        }
    }
}