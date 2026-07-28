using INest.Features.Locations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Locations.Queries.GetLocationHeader
{
    public class GetLocationHeaderHandler : IRequestHandler<GetLocationHeaderQuery, LocationHeaderDto?>
    {
        private readonly AppDbContext _context;

        public GetLocationHeaderHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LocationHeaderDto?> Handle(GetLocationHeaderQuery request, CancellationToken cancellationToken)
        {
            var location = await _context.StorageLocations
                .Where(l => l.UserId == request.UserId && l.Id == request.LocationId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (location == null) return null;

            ParentLocationDto? parentDto = null;

            if (location.ParentLocationId.HasValue)
            {
                parentDto = await _context.StorageLocations
                    .Where(l => l.Id == location.ParentLocationId.Value)
                    .Select(l => new ParentLocationDto
                    {
                        Id = l.Id,
                        Name = l.Name
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return new LocationHeaderDto
            {
                Id = location.Id,
                Name = location.Name,
                Description = location.Description,
                Color = location.Color,
                Icon = location.Icon,
                ParentLocation = parentDto
            };
        }
    }
}
