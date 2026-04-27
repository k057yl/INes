using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Locations.Commands.ReorderLocations
{
    public class ReorderLocationsHandler : IRequestHandler<ReorderLocationsCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public ReorderLocationsHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(ReorderLocationsCommand request, CancellationToken cancellationToken)
        {
            var locations = await _context.StorageLocations
                .Where(l => l.UserId == request.UserId && l.ParentLocationId == request.ParentId)
                .ToListAsync(cancellationToken);

            foreach (var loc in locations)
            {
                var index = request.OrderedIds.IndexOf(loc.Id);
                if (index != -1) loc.SortOrder = index;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}