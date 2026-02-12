using INest.Models.DTOs.Location;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;

namespace INest.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LocationService(AppDbContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<StorageLocation> CreateLocationAsync(Guid userId, CreateLocationDto dto)
        {
            if (dto.ParentLocationId.HasValue)
            {
                var parentExists = await _context.StorageLocations
                    .AnyAsync(l => l.Id == dto.ParentLocationId && l.UserId == userId);

                if (!parentExists)
                    throw new InvalidOperationException(_localizer["Error.LocationNotFound"]);
            }

            var location = new StorageLocation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                ParentLocationId = dto.ParentLocationId,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.StorageLocations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<IEnumerable<object>> GetUserLocationsAsync(Guid userId)
        {
            return await _context.StorageLocations
                .Where(l => l.UserId == userId)
                .OrderBy(l => l.SortOrder)
                .Select(l => new {
                    l.Id,
                    l.Name,
                    l.Description
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateSortOrderAsync(Guid userId, Guid locationId, int newOrder)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);

            if (location == null) return false;

            location.SortOrder = newOrder;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
