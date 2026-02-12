using INest.Models.DTOs.Location;
using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface ILocationService
    {
        Task<StorageLocation> CreateLocationAsync(Guid userId, CreateLocationDto dto);
        Task<IEnumerable<object>> GetUserLocationsAsync(Guid userId);
        Task<bool> UpdateSortOrderAsync(Guid userId, Guid locationId, int newOrder);
    }
}
