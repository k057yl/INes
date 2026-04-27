using INest.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Locations.Queries.GetLocations
{
    public class GetLocationsHandler : IRequestHandler<GetLocationsQuery, IEnumerable<object>>
    {
        private readonly AppDbContext _context;

        public GetLocationsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            return await _context.StorageLocations
                .Where(l => l.UserId == request.UserId)
                .AsNoTracking()
                .OrderBy(l => l.SortOrder)
                .Select(l => new {
                    l.Id,
                    l.Name,
                    l.Description,
                    l.Color,
                    l.Icon,
                    l.ParentLocationId,
                    ItemsCount = l.Items.Count(i => i.Status != ItemStatus.Sold),
                    items = l.Items
                        .Where(i => i.Status != ItemStatus.Sold)
                        .Select(i => new { i.Id, i.Name })
                        .ToList()
                })
                .ToListAsync(cancellationToken);
        }
    }
}