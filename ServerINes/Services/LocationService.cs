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
                Color = dto.Color ?? "#007bff",
                Icon = dto.Icon ?? "fa-folder",
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
                    l.Description,
                    l.Color,
                    l.Icon,
                    items = l.Items.Select(i => new { i.Id, i.Name }).ToList()
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
        //***********************
        public async Task MoveLocationAsync(Guid userId, Guid locationId, Guid? newParentId)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);

            if (location == null)
                throw new Exception("Location not found");

            if (locationId == newParentId)
                throw new Exception("Нельзя вложить локацию в саму себя");

            var parent = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == newParentId && l.UserId == userId);

            while (parent != null)
            {
                if (parent.Id == locationId)
                    throw new Exception("Нельзя вложить родителя внутрь потомка");

                parent = await _context.StorageLocations
                    .FirstOrDefaultAsync(l => l.Id == parent.ParentLocationId && l.UserId == userId);
            }

            location.ParentLocationId = newParentId;

            await _context.SaveChangesAsync();
        }

        public async Task ReorderLocationsAsync(Guid userId, Guid? parentId, List<Guid> orderedIds)
        {
            var locations = await _context.StorageLocations
                .Where(l => l.UserId == userId && l.ParentLocationId == parentId)
                .ToListAsync();

            for (int i = 0; i < orderedIds.Count; i++)
            {
                var loc = locations.FirstOrDefault(x => x.Id == orderedIds[i]);
                if (loc != null)
                    loc.SortOrder = i;
            }

            await _context.SaveChangesAsync();
        }

        private List<StorageLocation> BuildTree(List<StorageLocation> all, Guid? parentId)
        {
            return all
                .Where(l => l.ParentLocationId == parentId)
                .OrderBy(l => l.SortOrder)
                .Select(l => new StorageLocation
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Name = l.Name,
                    Description = l.Description,
                    Color = l.Color,
                    Icon = l.Icon,
                    ParentLocationId = l.ParentLocationId,
                    SortOrder = l.SortOrder,
                    CreatedAt = l.CreatedAt,
                    Items = l.Items,
                    Children = BuildTree(all, l.Id)
                })
                .ToList();
        }

        public async Task<List<StorageLocation>> GetTreeAsync(Guid userId)
        {
            var all = await _context.StorageLocations
                .Where(l => l.UserId == userId)
                .Include(l => l.Items)
                .ToListAsync();

            return BuildTree(all, null);
        }

        public async Task RenameLocationAsync(Guid userId, Guid locationId, string newName)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);

            if (location == null)
                throw new KeyNotFoundException(_localizer["Error.LocationNotFound"]);

            location.Name = newName;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLocationAsync(Guid userId, Guid locationId)
        {
            var location = await _context.StorageLocations
                .Include(l => l.Children)
                .FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);

            if (location == null)
                throw new KeyNotFoundException(_localizer["Error.LocationNotFound"]);

            _context.StorageLocations.Remove(location);
            await _context.SaveChangesAsync();
        }

        public async Task<StorageLocation?> GetLocationByIdAsync(Guid userId, Guid locationId)
        {
            return await _context.StorageLocations
                .Where(l => l.UserId == userId && l.Id == locationId)
                .Include(l => l.Items)
                .Include(l => l.Children)
                .FirstOrDefaultAsync();
        }
    }
}
