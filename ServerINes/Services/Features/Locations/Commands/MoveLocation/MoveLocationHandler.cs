using INest.Exceptions;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Locations.Commands.MoveLocation
{
    public class MoveLocationHandler : IRequestHandler<MoveLocationCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public MoveLocationHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(MoveLocationCommand request, CancellationToken cancellationToken)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == request.LocationId && l.UserId == request.UserId, cancellationToken);

            if (location == null)
                throw new KeyNotFoundException(LOCATIONS.ERRORS.NOT_FOUND);

            if (request.LocationId == request.NewParentId)
                throw new InvalidOperationException(LOCATIONS.ERRORS.SELF_NESTING);

            int movingSubtreeDepth = await GetSubtreeDepthAsync(request.UserId, request.LocationId, cancellationToken);
            int targetLevel = await GetLocationLevelAsync(request.UserId, request.NewParentId, cancellationToken);

            if (targetLevel + movingSubtreeDepth > 3)
                throw new AppException(ERRORS.MAX_NESTING_REACHED, 400);

            if (request.NewParentId.HasValue)
            {
                var currentParentId = request.NewParentId;
                while (currentParentId.HasValue)
                {
                    if (currentParentId == request.LocationId)
                        throw new InvalidOperationException(LOCATIONS.ERRORS.CIRCULAR_DEPENDENCY);

                    currentParentId = await _context.StorageLocations
                        .AsNoTracking()
                        .Where(l => l.Id == currentParentId && l.UserId == request.UserId)
                        .Select(l => l.ParentLocationId)
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }

            var maxSortOrder = await _context.StorageLocations
                .Where(l => l.UserId == request.UserId && l.ParentLocationId == request.NewParentId)
                .MaxAsync(l => (int?)l.SortOrder, cancellationToken) ?? -1;

            location.ParentLocationId = request.NewParentId;
            location.SortOrder = maxSortOrder + 1;

            await _context.SaveChangesAsync(cancellationToken);
            _tracker.InvalidateUserCache(request.UserId);

            return true;
        }

        private async Task<int> GetLocationLevelAsync(Guid userId, Guid? locationId, CancellationToken cancellationToken)
        {
            if (!locationId.HasValue) return 0;
            int level = 0;
            var currentId = locationId;

            while (currentId.HasValue)
            {
                level++;
                currentId = await _context.StorageLocations
                    .AsNoTracking()
                    .Where(l => l.Id == currentId && l.UserId == userId)
                    .Select(l => l.ParentLocationId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (level > 10) break;
            }
            return level;
        }

        private async Task<int> GetSubtreeDepthAsync(Guid userId, Guid locationId, CancellationToken cancellationToken)
        {
            var allUserLocs = await _context.StorageLocations
                .Where(l => l.UserId == userId)
                .AsNoTracking()
                .Select(l => new { l.Id, l.ParentLocationId })
                .ToListAsync(cancellationToken);

            int GetMaxDepth(Guid id)
            {
                var children = allUserLocs.Where(l => l.ParentLocationId == id).ToList();
                if (!children.Any()) return 1;
                return 1 + children.Max(c => GetMaxDepth(c.Id));
            }

            return GetMaxDepth(locationId);
        }
    }
}