using INest.Models.DTOs.Platform;
using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface IPlatformService
    {
        Task<IEnumerable<Platform>> GetAllAsync(Guid userId);
        Task<Platform> CreateAsync(Guid userId, PlatformDto dto);
        Task<Platform?> UpdateAsync(Guid userId, Guid id, PlatformDto dto);
        Task<bool> DeleteAsync(Guid userId, Guid id);
    }
}
