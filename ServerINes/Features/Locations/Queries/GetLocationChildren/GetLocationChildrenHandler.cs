using INest.Data.Enums;
using INest.Features.Locations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Locations.Queries.GetLocationChildren
{
    public class GetLocationChildrenHandler : IRequestHandler<GetLocationChildrenQuery, IEnumerable<LocationChildDto>>
    {
        private readonly AppDbContext _context;

        public GetLocationChildrenHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LocationChildDto>> Handle(GetLocationChildrenQuery request, CancellationToken cancellationToken)
        {
            return await _context.StorageLocations
                .Where(l => l.UserId == request.UserId && l.ParentLocationId == request.LocationId)
                .AsNoTracking()
                .OrderBy(l => l.SortOrder)
                .Select(l => new LocationChildDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Color = l.Color,
                    Icon = l.Icon,
                    ItemsCount = l.Items.Count(i => i.Status != ItemStatus.Sold)
                })
                .ToListAsync(cancellationToken);
        }
    }
}
