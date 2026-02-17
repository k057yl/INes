using INest.Models.DTOs.Item;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace INest.Services
{
    public class ItemService : IItemService
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;

        public ItemService(AppDbContext context, IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        public async Task<Item> CreateItemAsync(Guid userId, CreateItemDto dto, IFormFile? photo)
        {
            var item = new Item
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                StorageLocationId = dto.StorageLocationId,
                Status = dto.Status,
                PurchaseDate = dto.PurchaseDate,
                PurchasePrice = dto.PurchasePrice,
                EstimatedValue = dto.EstimatedValue,
                CreatedAt = DateTime.UtcNow
            };

            _context.Items.Add(item);

            if (photo != null && photo.Length > 0)
            {
                var result = await _photoService.AddPhotoAsync(photo);

                if (result.Error == null)
                {
                    item.PhotoUrl = result.SecureUrl.ToString();
                    item.PublicId = result.PublicId;

                    _context.ItemPhotos.Add(new ItemPhoto
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        FilePath = item.PhotoUrl
                    });
                }
            }

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                Type = ItemHistoryType.Created,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<IEnumerable<Item>> GetUserItemsAsync(Guid userId)
        {
            return await _context.Items
                .Where(i => i.UserId == userId)
                .Include(i => i.Photos)
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .ToListAsync();
        }

        public async Task<Item?> GetItemAsync(Guid userId, Guid itemId)
        {
            return await _context.Items
                .Where(i => i.UserId == userId && i.Id == itemId)
                .Include(i => i.Photos)
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateItemAsync(Guid userId, Guid itemId, UpdateItemDto dto)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);
            if (item == null) return false;

            void AddHistory(ItemHistoryType type, string? oldVal, string? newVal)
            {
                _context.ItemHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    Type = type,
                    OldValue = oldVal,
                    NewValue = newVal,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != item.Name)
                AddHistory(ItemHistoryType.ValueUpdated, item.Name, dto.Name);

            if (!string.IsNullOrEmpty(dto.Description) && dto.Description != item.Description)
                AddHistory(ItemHistoryType.ValueUpdated, item.Description, dto.Description);

            if (dto.CategoryId.HasValue && dto.CategoryId.Value != item.CategoryId)
                AddHistory(ItemHistoryType.ValueUpdated, item.CategoryId.ToString(), dto.CategoryId.Value.ToString());

            if (dto.StorageLocationId.HasValue && dto.StorageLocationId != item.StorageLocationId)
                AddHistory(ItemHistoryType.Moved, item.StorageLocationId?.ToString(), dto.StorageLocationId?.ToString());

            if (dto.PurchasePrice.HasValue && dto.PurchasePrice != item.PurchasePrice)
                AddHistory(ItemHistoryType.ValueUpdated, item.PurchasePrice?.ToString(), dto.PurchasePrice?.ToString());

            if (dto.EstimatedValue.HasValue && dto.EstimatedValue != item.EstimatedValue)
                AddHistory(ItemHistoryType.ValueUpdated, item.EstimatedValue?.ToString(), dto.EstimatedValue?.ToString());

            // Apply updates
            if (!string.IsNullOrEmpty(dto.Name)) item.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) item.Description = dto.Description;
            if (dto.CategoryId.HasValue) item.CategoryId = dto.CategoryId.Value;
            if (dto.StorageLocationId.HasValue) item.StorageLocationId = dto.StorageLocationId;
            if (dto.PurchasePrice.HasValue) item.PurchasePrice = dto.PurchasePrice;
            if (dto.EstimatedValue.HasValue) item.EstimatedValue = dto.EstimatedValue;
            if (dto.PurchaseDate.HasValue) item.PurchaseDate = dto.PurchaseDate;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MoveItemAsync(Guid userId, Guid itemId, Guid? targetLocationId)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);
            if (item == null) return false;

            if (item.StorageLocationId != targetLocationId)
            {
                _context.ItemHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    Type = ItemHistoryType.Moved,
                    OldValue = item.StorageLocationId?.ToString(),
                    NewValue = targetLocationId?.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                var oldStatus = item.Status;

                if (targetLocationId.HasValue)
                {
                    var targetLocation = await _context.StorageLocations
                        .FirstOrDefaultAsync(l => l.Id == targetLocationId.Value);

                    if (targetLocation != null)
                    {
                        if (targetLocation.IsSalesLocation)
                        {
                            item.Status = ItemStatus.Listed;
                        }
                        if (targetLocation.IsLendingLocation)
                        {
                            item.Status = ItemStatus.Lent;
                        }
                        else if (item.Status == ItemStatus.Listed)
                        {
                            item.Status = ItemStatus.Active;
                        }
                    }
                }
                else if (item.Status == ItemStatus.Listed)
                {
                    item.Status = ItemStatus.Active;
                }

                if (oldStatus != item.Status)
                {
                    _context.ItemHistories.Add(new ItemHistory
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        Type = ItemHistoryType.StatusChanged,
                        OldValue = oldStatus.ToString(),
                        NewValue = item.Status.ToString(),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                item.StorageLocationId = targetLocationId;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> ChangeStatusAsync(Guid userId, Guid itemId, ItemStatus newStatus)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);
            if (item == null) return false;

            if (item.Status != newStatus)
            {
                _context.ItemHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    Type = ItemHistoryType.StatusChanged,
                    OldValue = item.Status.ToString(),
                    NewValue = newStatus.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                item.Status = newStatus;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<IEnumerable<ItemHistory>> GetItemHistoryAsync(Guid userId, Guid itemId)
        {
            var exists = await _context.Items.AnyAsync(i => i.Id == itemId && i.UserId == userId);
            if (!exists) return Enumerable.Empty<ItemHistory>();

            return await _context.ItemHistories
                .Where(h => h.ItemId == itemId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteItemAsync(Guid userId, Guid itemId)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);
            if (item == null) return false;

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
