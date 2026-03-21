using INest.Constants;
using INest.Exceptions;
using INest.Models.DTOs.Location;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;

        public LocationService(AppDbContext context) => _context = context;

        public async Task<StorageLocation> CreateLocationAsync(Guid userId, CreateLocationDto dto)
        {
            if (dto.ParentLocationId.HasValue)
            {
                var parentExists = await _context.StorageLocations
                    .AnyAsync(l => l.Id == dto.ParentLocationId && l.UserId == userId);

                if (!parentExists)
                    throw new AppException(LocalizationConstants.LOCATIONS.NOT_FOUND, 404);
            }

            var location = new StorageLocation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                ParentLocationId = dto.ParentLocationId,
                SortOrder = dto.SortOrder,
                Color = dto.Color ?? "#007bff",
                Icon = dto.Icon ?? "fa-folder",
                CreatedAt = DateTime.UtcNow,
                IsSalesLocation = dto.IsSalesLocation,
                IsLendingLocation = dto.IsLendingLocation
            };

            _context.StorageLocations.Add(location);
            await _context.SaveChangesAsync();

            return location;
        }

        public async Task<IEnumerable<object>> GetUserLocationsAsync(Guid userId)
        {
            return await _context.StorageLocations
                .Where(l => l.UserId == userId)
                .AsNoTracking()
                .OrderBy(l => l.SortOrder)
                .Select(l => new {
                    l.Id,
                    l.Name,
                    l.Description,
                    l.Color,
                    l.Icon,
                    l.IsSalesLocation,
                    l.IsLendingLocation,
                    items = l.Items
                    .Where(i => i.Status != ItemStatus.Sold)
                    .Select(i => new { i.Id, i.Name })
                    .ToList()
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

        public async Task MoveLocationAsync(Guid userId, Guid locationId, Guid? newParentId)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);

            if (location == null)
                throw new KeyNotFoundException(LocalizationConstants.LOCATIONS.NOT_FOUND);

            if (locationId == newParentId)
                throw new InvalidOperationException(LocalizationConstants.LOCATIONS.SELF_NESTING);

            if (newParentId.HasValue)
            {
                var currentParentId = newParentId;
                while (currentParentId.HasValue)
                {
                    if (currentParentId == locationId)
                        throw new InvalidOperationException(LocalizationConstants.LOCATIONS.CIRCULAR_DEPENDENCY);

                    currentParentId = await _context.StorageLocations
                        .AsNoTracking()
                        .Where(l => l.Id == currentParentId && l.UserId == userId)
                        .Select(l => l.ParentLocationId)
                        .FirstOrDefaultAsync();
                }
            }

            location.ParentLocationId = newParentId;
            await _context.SaveChangesAsync();
        }

        public async Task ReorderLocationsAsync(Guid userId, Guid? parentId, List<Guid> orderedIds)
        {
            var locations = await _context.StorageLocations
                .Where(l => l.UserId == userId && l.ParentLocationId == parentId)
                .ToListAsync();

            foreach (var loc in locations)
            {
                var index = orderedIds.IndexOf(loc.Id);
                if (index != -1) loc.SortOrder = index;
            }

            await _context.SaveChangesAsync();
        }

        private List<StorageLocation> BuildTree(List<StorageLocation> all, Guid? parentId)
        {
            return all
                .Where(l => l.ParentLocationId == parentId)
                .OrderBy(l => l.SortOrder)
                .Select(l => {
                    l.Children = BuildTree(all, l.Id);
                    return l;
                })
                .ToList();
        }

        public async Task<List<StorageLocation>> GetTreeAsync(Guid userId)
        {
            var all = await _context.StorageLocations
                .Where(l => l.UserId == userId)
                .AsNoTracking()
                .Include(l => l.Items.Where(i => i.Status != ItemStatus.Sold))
                    .ThenInclude(i => i.Category)
                .ToListAsync();

            return BuildTree(all, null);
        }

        public async Task RenameLocationAsync(Guid userId, Guid locationId, string newName)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);

            if (location == null)
                throw new KeyNotFoundException(LocalizationConstants.LOCATIONS.NOT_FOUND);

            location.Name = newName;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLocationAsync(Guid userId, Guid locationId)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);

            if (location == null)
                throw new KeyNotFoundException(LocalizationConstants.LOCATIONS.NOT_FOUND);

            _context.StorageLocations.Remove(location);
            await _context.SaveChangesAsync();
        }

        public async Task<StorageLocation?> GetLocationByIdAsync(Guid userId, Guid locationId)
        {
            return await _context.StorageLocations
                .Where(l => l.UserId == userId && l.Id == locationId)
                .AsNoTracking()
                .Include(l => l.Items.Where(i => i.Status != ItemStatus.Sold))
                .Include(l => l.Children)
                .FirstOrDefaultAsync();
        }
    }
}