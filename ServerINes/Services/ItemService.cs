using INest.Models.DTOs.Item;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Item> CreateItemAsync(Guid userId, CreateItemDto dto, List<IFormFile> photos)
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
                CreatedAt = DateTime.UtcNow,
                Photos = new List<ItemPhoto>()
            };

            if (photos != null && photos.Count > 0)
            {
                foreach (var photo in photos)
                {
                    var result = await _photoService.AddPhotoAsync(photo);
                    if (result.Error == null)
                    {
                        var itemPhoto = new ItemPhoto
                        {
                            Id = Guid.NewGuid(),
                            ItemId = item.Id,
                            FilePath = result.SecureUrl.ToString(),
                            PublicId = result.PublicId
                        };

                        if (string.IsNullOrEmpty(item.PhotoUrl))
                        {
                            item.PhotoUrl = itemPhoto.FilePath;
                            item.PublicId = itemPhoto.PublicId;
                        }

                        item.Photos.Add(itemPhoto);
                    }
                }
            }

            _context.Items.Add(item);

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

            if (dto.Name != null && dto.Name != item.Name)
                AddHistory(ItemHistoryType.ValueUpdated, item.Name, dto.Name);

            if (dto.Description != item.Description)
                AddHistory(ItemHistoryType.ValueUpdated, item.Description, dto.Description);

            if (dto.CategoryId.HasValue && dto.CategoryId.Value != item.CategoryId)
                AddHistory(ItemHistoryType.ValueUpdated, item.CategoryId.ToString(), dto.CategoryId.Value.ToString());

            if (dto.StorageLocationId != item.StorageLocationId)
                AddHistory(ItemHistoryType.Moved, item.StorageLocationId?.ToString(), dto.StorageLocationId?.ToString());

            if (dto.PurchasePrice != item.PurchasePrice)
                AddHistory(ItemHistoryType.ValueUpdated, item.PurchasePrice?.ToString(), dto.PurchasePrice?.ToString());

            if (dto.EstimatedValue != item.EstimatedValue)
                AddHistory(ItemHistoryType.ValueUpdated, item.EstimatedValue?.ToString(), dto.EstimatedValue?.ToString());

            if (dto.PurchaseDate != item.PurchaseDate)
                AddHistory(ItemHistoryType.ValueUpdated, item.PurchaseDate?.ToString(), dto.PurchaseDate?.ToString());

            if (dto.Name != null) item.Name = dto.Name;
            item.Description = dto.Description;
            if (dto.CategoryId.HasValue) item.CategoryId = dto.CategoryId.Value;
            item.StorageLocationId = dto.StorageLocationId;
            item.PurchasePrice = dto.PurchasePrice;
            item.EstimatedValue = dto.EstimatedValue;
            item.PurchaseDate = dto.PurchaseDate;

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
                        if (targetLocation.IsSalesLocation) item.Status = ItemStatus.Listed;
                        else if (targetLocation.IsLendingLocation) item.Status = ItemStatus.Lent;
                        else if (item.Status == ItemStatus.Listed || item.Status == ItemStatus.Lent)
                            item.Status = ItemStatus.Active;
                    }
                }
                else if (item.Status == ItemStatus.Listed || item.Status == ItemStatus.Lent)
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

        public async Task<bool> CancelSaleAsync(Guid userId, Guid itemId)
        {
            var item = await _context.Items
                .Include(i => i.Sale)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

            if (item == null || item.Sale == null) return false;

            _context.Sales.Remove(item.Sale);
            item.Status = ItemStatus.Active;

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                Type = ItemHistoryType.StatusChanged,
                OldValue = "Sold",
                NewValue = "Active",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid itemId)
        {
            var item = await _context.Items
                .Include(i => i.Photos)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

            if (item == null) return false;

            foreach (var photo in item.Photos)
            {
                if (!string.IsNullOrEmpty(photo.PublicId))
                    await _photoService.DeletePhotoAsync(photo.PublicId);
            }

            var history = await _context.ItemHistories.Where(h => h.ItemId == itemId).ToListAsync();
            _context.ItemHistories.RemoveRange(history);

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
