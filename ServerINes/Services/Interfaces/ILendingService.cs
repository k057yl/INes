using INest.Models.DTOs.Lending;
using INest.Models.Entities;
using INest.Models.Enums;

namespace INest.Services.Interfaces
{
    public interface ILendingService
    {
        Task<Lending> LendItemAsync(Guid userId, LendItemDto dto);
        Task<bool> ReturnItemAsync(Guid userId, Guid itemId, ReturnItemDto dto);
        Task SyncLendingStateAsync(Item item, ItemStatus newStatus, string personName, string? contactEmail, DateTime? expectedReturnDate, bool sendNotification);
    }
}
