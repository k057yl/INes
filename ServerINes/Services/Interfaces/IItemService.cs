using INest.Models.DTOs.Item;
using INest.Models.Entities;
using INest.Models.Enums;

namespace INest.Services.Interfaces
{
    public interface IItemService
    {
        Task<Item> CreateItemAsync(Guid userId, CreateItemDto dto, List<IFormFile> photos);
        Task<IEnumerable<Item>> GetUserItemsAsync(Guid userId, ItemFilterDto filters);
        Task<Item?> GetItemAsync(Guid userId, Guid itemId);
        Task<bool> UpdateFullAsync(Guid userId, Guid itemId, UpdateItemFullDto dto, List<IFormFile>? photos);
        Task<bool> UpdatePartialAsync(Guid userId, Guid itemId, UpdateItemPartialDto dto, List<IFormFile>? photos);
        Task<bool> DeleteAsync(Guid userId, Guid itemId);
        Task<bool> MoveItemAsync(Guid userId, Guid itemId, Guid? targetLocationId);
        Task<bool> ChangeStatusAsync(Guid userId, Guid itemId, ItemStatus newStatus);
        Task<IEnumerable<ItemHistory>> GetItemHistoryAsync(Guid userId, Guid itemId);
        Task<bool> CancelSaleAsync(Guid userId, Guid itemId);
        Task<bool> DeleteBatchAsync(Guid userId, List<Guid> itemIds);
    }
}
