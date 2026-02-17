using INest.Models.DTOs.Item;
using INest.Models.Entities;
using INest.Models.Enums;

namespace INest.Services.Interfaces
{
    public interface IItemService
    {
        Task<Item> CreateItemAsync(Guid userId, CreateItemDto dto, IFormFile? photo);
        Task<IEnumerable<Item>> GetUserItemsAsync(Guid userId);
        Task<Item?> GetItemAsync(Guid userId, Guid itemId);
        Task<bool> UpdateItemAsync(Guid userId, Guid itemId, UpdateItemDto dto);
        Task<bool> DeleteItemAsync(Guid userId, Guid itemId);

        Task<bool> MoveItemAsync(Guid userId, Guid itemId, Guid? targetLocationId);
        Task<bool> ChangeStatusAsync(Guid userId, Guid itemId, ItemStatus newStatus);
        Task<IEnumerable<ItemHistory>> GetItemHistoryAsync(Guid userId, Guid itemId);
        Task<bool> CancelSaleAsync(Guid userId, Guid itemId);
    }
}
