using INest.Models.Enums;
using INest.Services.Features.Locations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Locations.Queries.GetLocationById
{
    public class GetLocationByIdHandler : IRequestHandler<GetLocationByIdQuery, StorageLocationDetailDto?>
    {
        private readonly AppDbContext _context;

        public GetLocationByIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StorageLocationDetailDto?> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.UserId == request.UserId && l.Id == request.LocationId, cancellationToken);

            if (location == null) return null;

            var itemsCount = await _context.Items
                .CountAsync(i => i.StorageLocationId == location.Id && i.Status != ItemStatus.Sold, cancellationToken);

            var items = await _context.Items
                .Include(i => i.Category)
                .Where(i => i.StorageLocationId == location.Id && i.Status != ItemStatus.Sold)
                .ToListAsync(cancellationToken);

            var children = await _context.StorageLocations
                .Where(l => l.ParentLocationId == location.Id)
                .OrderBy(l => l.SortOrder)
                .ToListAsync(cancellationToken);

            return new StorageLocationDetailDto
            {
                Id = location.Id,
                UserId = location.UserId,
                Name = location.Name,
                Description = location.Description,
                SortOrder = location.SortOrder,
                IsSalesLocation = location.IsSalesLocation,
                IsLendingLocation = location.IsLendingLocation,

                ItemsCount = itemsCount,
                Items = items,
                Children = children
            };
        }
    }
}