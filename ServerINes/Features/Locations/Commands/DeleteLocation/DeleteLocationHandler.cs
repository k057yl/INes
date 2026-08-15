using INest.Data.Entities.Core;
using INest.Exceptions;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Locations.Commands.DeleteLocation
{
    public class DeleteLocationHandler : IRequestHandler<DeleteLocationCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public DeleteLocationHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == request.Id && l.UserId == request.UserId, cancellationToken);

            if (location == null)
            {
                throw new AppException(LOCATIONS.ERRORS.NOT_FOUND, 404);
            }

            var hasChildren = await _context.StorageLocations
                .AnyAsync(l => l.ParentLocationId == location.Id, cancellationToken);

            var hasItems = await _context.Items
                .AnyAsync(i => i.StorageLocationId == location.Id, cancellationToken);

            if (hasChildren || hasItems)
            {
                Guid destinationId;

                if (request.TargetLocationId.HasValue && request.TargetLocationId.Value != Guid.Empty)
                {
                    destinationId = request.TargetLocationId.Value;

                    if (destinationId == request.Id)
                    {
                        throw new AppException(LOCATIONS.ERRORS.SELF_NESTING, 400);
                    }

                    var targetExists = await _context.StorageLocations
                        .AnyAsync(l => l.Id == destinationId && l.UserId == request.UserId, cancellationToken);

                    if (!targetExists)
                    {
                        destinationId = await GetOrCreateOtherLocationAsync(request.UserId, cancellationToken);
                    }
                }
                else
                {
                    destinationId = await GetOrCreateOtherLocationAsync(request.UserId, cancellationToken);
                }

                var itemsToMove = await _context.Items
                    .Where(i => i.StorageLocationId == location.Id && i.UserId == request.UserId)
                    .ToListAsync(cancellationToken);

                foreach (var item in itemsToMove)
                {
                    item.StorageLocationId = destinationId;
                }

                var subLocationsToMove = await _context.StorageLocations
                    .Where(l => l.ParentLocationId == location.Id && l.UserId == request.UserId)
                    .ToListAsync(cancellationToken);

                foreach (var subLoc in subLocationsToMove)
                {
                    subLoc.ParentLocationId = destinationId;
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            _context.StorageLocations.Remove(location);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }

        private async Task<Guid> GetOrCreateOtherLocationAsync(Guid userId, CancellationToken ct)
        {
            var existingOther = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Name == "Other" && l.ParentLocationId == null, ct);

            if (existingOther != null)
            {
                return existingOther.Id;
            }

            var newOther = new StorageLocation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Other",
                ParentLocationId = null
            };

            _context.StorageLocations.Add(newOther);
            await _context.SaveChangesAsync(ct);

            return newOther.Id;
        }
    }
}