using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Locations.Commands.UpdateLocationSortOrder
{
    public class UpdateLocationSortOrderHandler : IRequestHandler<UpdateLocationSortOrderCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public UpdateLocationSortOrderHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(UpdateLocationSortOrderCommand request, CancellationToken cancellationToken)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == request.LocationId && l.UserId == request.UserId, cancellationToken);

            if (location == null) return false;

            location.SortOrder = request.NewOrder;
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}