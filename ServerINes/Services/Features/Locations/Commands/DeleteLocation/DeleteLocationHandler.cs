using INest.Exceptions;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Locations.Commands.DeleteLocation
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
                throw new AppException(ERRORS.NOT_EMPTY, 400);
            }

            _context.StorageLocations.Remove(location);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}