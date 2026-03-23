using INest.Models.DTOs.Lending;
using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface ILendingService
    {
        Task<Lending> LendItemAsync(Guid userId, LendItemDto dto);
        Task<bool> ReturnItemAsync(Guid userId, Guid itemId, ReturnItemDto dto);
    }
}
