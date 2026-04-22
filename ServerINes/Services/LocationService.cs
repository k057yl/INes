using FluentValidation;
using Ganss.Xss;
using INest.Exceptions;
using INest.Models.DTOs.Location;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly IValidator<CreateLocationDto> _locationValidator;

        public LocationService(
            AppDbContext context,
            IHtmlSanitizer sanitizer,
            IValidator<CreateLocationDto> locationValidator)
        {
            _context = context;
            _sanitizer = sanitizer;
            _locationValidator = locationValidator;
        }

        public async Task<StorageLocation> CreateLocationAsync(Guid userId, CreateLocationDto dto)
        {
            var valResult = await _locationValidator.ValidateAsync(dto);
            if (!valResult.IsValid) throw new ValidationAppException(valResult.Errors);

            if (dto.ParentLocationId.HasValue)
            {
                var parentExists = await _context.StorageLocations
                    .AnyAsync(l => l.Id == dto.ParentLocationId && l.UserId == userId);

                if (!parentExists)
                    throw new AppException(LOCATIONS.ERRORS.NOT_FOUND, 404);
            }

            var sanitizedName = _sanitizer.Sanitize(dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                throw new AppException(LOCATIONS.ERRORS.INVALID_NAME, 400);
            }

            var location = new StorageLocation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = sanitizedName,
                Description = !string.IsNullOrEmpty(dto.Description) ? _sanitizer.Sanitize(dto.Description) : null,
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
                    l.ParentLocationId,
                    ItemsCount = l.Items.Count(i => i.Status != ItemStatus.Sold),
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
                throw new KeyNotFoundException(LOCATIONS.ERRORS.NOT_FOUND);

            if (locationId == newParentId)
                throw new InvalidOperationException(LOCATIONS.ERRORS.SELF_NESTING);

            int movingSubtreeDepth = await GetSubtreeDepthAsync(userId, locationId);
            int targetLevel = await GetLocationLevelAsync(userId, newParentId);

            if (targetLevel + movingSubtreeDepth > 3)
                throw new AppException(ERRORS.MAX_NESTING_REACHED, 400);

            if (newParentId.HasValue)
            {
                var currentParentId = newParentId;
                while (currentParentId.HasValue)
                {
                    if (currentParentId == locationId)
                        throw new InvalidOperationException(LOCATIONS.ERRORS.CIRCULAR_DEPENDENCY);

                    currentParentId = await _context.StorageLocations
                        .AsNoTracking()
                        .Where(l => l.Id == currentParentId && l.UserId == userId)
                        .Select(l => l.ParentLocationId)
                        .FirstOrDefaultAsync();
                }
            }

            var maxSortOrder = await _context.StorageLocations
                .Where(l => l.UserId == userId && l.ParentLocationId == newParentId)
                .MaxAsync(l => (int?)l.SortOrder) ?? -1;

            location.ParentLocationId = newParentId;
            location.SortOrder = maxSortOrder + 1;

            await _context.SaveChangesAsync();
        }

        private async Task<int> GetLocationLevelAsync(Guid userId, Guid? locationId)
        {
            if (!locationId.HasValue) return 0;
            int level = 0;
            var currentId = locationId;

            while (currentId.HasValue)
            {
                level++;
                currentId = await _context.StorageLocations
                    .AsNoTracking()
                    .Where(l => l.Id == currentId && l.UserId == userId)
                    .Select(l => l.ParentLocationId)
                    .FirstOrDefaultAsync();

                if (level > 10) break;
            }
            return level;
        }

        private async Task<int> GetSubtreeDepthAsync(Guid userId, Guid locationId)
        {
            var allUserLocs = await _context.StorageLocations
                .Where(l => l.UserId == userId)
                .AsNoTracking()
                .Select(l => new { l.Id, l.ParentLocationId })
                .ToListAsync();

            int GetMaxDepth(Guid id)
            {
                var children = allUserLocs.Where(l => l.ParentLocationId == id).ToList();
                if (!children.Any()) return 1;
                return 1 + children.Max(c => GetMaxDepth(c.Id));
            }

            return GetMaxDepth(locationId);
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
                throw new KeyNotFoundException(LOCATIONS.ERRORS.NOT_FOUND);

            var sanitizedName = _sanitizer.Sanitize(newName);
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                throw new AppException(LOCATIONS.ERRORS.INVALID_NAME, 400);
            }

            location.Name = sanitizedName;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLocationAsync(Guid userId, Guid locationId)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);

            if (location == null)
                throw new KeyNotFoundException(LOCATIONS.ERRORS.NOT_FOUND);

            _context.StorageLocations.Remove(location);
            await _context.SaveChangesAsync();
        }

        public async Task<StorageLocation?> GetLocationByIdAsync(Guid userId, Guid locationId)
        {
            var data = await _context.StorageLocations
                .Where(l => l.UserId == userId && l.Id == locationId)
                .Select(l => new
                {
                    Location = l,
                    CurrentItemsCount = l.Items.Count(i => i.Status != ItemStatus.Sold),
                    ChildrenData = l.Children.Select(c => new
                    {
                        Child = c,
                        Count = c.Items.Count(i => i.Status != ItemStatus.Sold)
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null) return null;

            var location = data.Location;
            location.ItemsCount = data.CurrentItemsCount;

            location.Children = data.ChildrenData.Select(cd =>
            {
                cd.Child.ItemsCount = cd.Count;
                return cd.Child;
            }).ToList();

            location.ParentLocation = await GetParentChainAsync(userId, location.ParentLocationId);

            await _context.Entry(location)
                .Collection(l => l.Items)
                .Query()
                .Where(i => i.Status != ItemStatus.Sold)
                .Include(i => i.Category)
                .LoadAsync();

            return location;
        }

        private async Task<StorageLocation?> GetParentChainAsync(Guid userId, Guid? parentId)
        {
            if (!parentId.HasValue) return null;

            var parent = await _context.StorageLocations
                .Where(l => l.UserId == userId && l.Id == parentId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (parent != null)
            {
                parent.ParentLocation = await GetParentChainAsync(userId, parent.ParentLocationId);
            }

            return parent;
        }
    }
}