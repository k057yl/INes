using INest.Models.DTOs.Location;
using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface ILocationService
    {
        Task<StorageLocation> CreateLocationAsync(Guid userId, CreateLocationDto dto);
        Task<IEnumerable<object>> GetUserLocationsAsync(Guid userId);
        Task<bool> UpdateSortOrderAsync(Guid userId, Guid locationId, int newOrder);
        Task MoveLocationAsync(Guid userId, Guid locationId, Guid? newParentId);
        Task ReorderLocationsAsync(Guid userId, Guid? parentId, List<Guid> orderedIds);
        Task<List<StorageLocation>> GetTreeAsync(Guid userId);
        Task RenameLocationAsync(Guid userId, Guid locationId, string newName);
        Task DeleteLocationAsync(Guid userId, Guid locationId);
        Task<StorageLocation?> GetLocationByIdAsync(Guid userId, Guid locationId);
    }
}
