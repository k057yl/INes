using INest.Models.DTOs.Item;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using INest.Constants;

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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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
                    var uploadTasks = photos.Select(p => _photoService.AddPhotoAsync(p)).ToList();
                    var results = await Task.WhenAll(uploadTasks);

                    foreach (var result in results)
                    {
                        if (result.Error == null)
                        {
                            var itemPhoto = new ItemPhoto
                            {
                                Id = Guid.NewGuid(),
                                ItemId = item.Id,
                                FilePath = result.SecureUrl.ToString(),
                                PublicId = result.PublicId,
                                UploadedAt = DateTime.UtcNow
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
                await transaction.CommitAsync();

                return item;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Item>> GetUserItemsAsync(Guid userId, ItemFilterDto filters)
        {
            var query = _context.Items
                .Where(i => i.UserId == userId)
                .Include(i => i.Photos)
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
            {
                var search = filters.SearchQuery.Trim().ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(search) ||
                                        (i.Description != null && i.Description.ToLower().Contains(search)));
            }

            if (filters.CategoryId.HasValue) query = query.Where(i => i.CategoryId == filters.CategoryId);
            if (filters.Status.HasValue) query = query.Where(i => i.Status == filters.Status);
            if (filters.MinPrice.HasValue) query = query.Where(i => i.PurchasePrice >= filters.MinPrice);
            if (filters.MaxPrice.HasValue) query = query.Where(i => i.PurchasePrice <= filters.MaxPrice);

            query = filters.SortBy switch
            {
                ItemSortOption.NameAsc => query.OrderBy(i => i.Name),
                ItemSortOption.NameDesc => query.OrderByDescending(i => i.Name),
                ItemSortOption.PriceAsc => query.OrderBy(i => i.PurchasePrice),
                ItemSortOption.PriceDesc => query.OrderByDescending(i => i.PurchasePrice),
                ItemSortOption.Oldest => query.OrderBy(i => i.CreatedAt),
                _ => query.OrderByDescending(i => i.CreatedAt)
            };

            return await query.ToListAsync();
        }

        public async Task<Item?> GetItemAsync(Guid userId, Guid itemId)
        {
            var item = await _context.Items
                .Where(i => i.UserId == userId && i.Id == itemId)
                .Include(i => i.Photos)
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .Include(i => i.Sale)
                .FirstOrDefaultAsync();

            if (item == null)
                throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

            return item;
        }

        public async Task<bool> UpdateFullAsync(Guid userId, Guid itemId, UpdateItemFullDto dto, List<IFormFile>? photos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var item = await _context.Items
                    .Include(i => i.Photos)
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

                if (item == null)
                    throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

                if (item.Status != ItemStatus.Active)
                    throw new InvalidOperationException(LocalizationConstants.SYSTEM.VALIDATION_FAILED);

                item.Name = dto.Name;
                item.Description = dto.Description;
                item.CategoryId = dto.CategoryId;
                item.StorageLocationId = dto.StorageLocationId;
                item.PurchaseDate = dto.PurchaseDate;
                item.PurchasePrice = dto.PurchasePrice;
                item.EstimatedValue = dto.EstimatedValue;

                await HandlePhotos(item, photos);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdatePartialAsync(Guid userId, Guid itemId, UpdateItemPartialDto dto, List<IFormFile>? photos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var item = await _context.Items
                    .Include(i => i.Photos)
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

                if (item == null)
                    throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

                if (item.Status != ItemStatus.Active)
                    throw new InvalidOperationException("Only items with 'Active' status can be edited. Cancel active processes first.");

                void LogChange(ItemHistoryType type, string? oldValue, string? newValue)
                {
                    if (oldValue == newValue) return;
                    _context.ItemHistories.Add(new ItemHistory
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        Type = type,
                        OldValue = oldValue ?? string.Empty,
                        NewValue = newValue ?? string.Empty,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (dto.Name != null && dto.Name != item.Name)
                {
                    LogChange(ItemHistoryType.ValueUpdated, item.Name, dto.Name);
                    item.Name = dto.Name;
                }

                if (dto.Description != null && dto.Description != item.Description)
                {
                    LogChange(ItemHistoryType.ValueUpdated, item.Description, dto.Description);
                    item.Description = dto.Description;
                }

                if (dto.CategoryId.HasValue && dto.CategoryId.Value != item.CategoryId)
                {
                    LogChange(ItemHistoryType.ValueUpdated, item.CategoryId.ToString(), dto.CategoryId.Value.ToString());
                    item.CategoryId = dto.CategoryId.Value;
                }

                if (dto.StorageLocationId.HasValue && dto.StorageLocationId.Value != item.StorageLocationId)
                {
                    LogChange(ItemHistoryType.Moved, item.StorageLocationId?.ToString(), dto.StorageLocationId.Value.ToString());
                    item.StorageLocationId = dto.StorageLocationId.Value;
                }

                if (dto.PurchaseDate.HasValue && dto.PurchaseDate != item.PurchaseDate)
                {
                    item.PurchaseDate = dto.PurchaseDate;
                }

                if (dto.PurchasePrice.HasValue && dto.PurchasePrice != item.PurchasePrice)
                {
                    LogChange(ItemHistoryType.ValueUpdated, item.PurchasePrice?.ToString(), dto.PurchasePrice.Value.ToString());
                    item.PurchasePrice = dto.PurchasePrice;
                }

                if (dto.EstimatedValue.HasValue && dto.EstimatedValue != item.EstimatedValue)
                {
                    LogChange(ItemHistoryType.ValueUpdated, item.EstimatedValue?.ToString(), dto.EstimatedValue.Value.ToString());
                    item.EstimatedValue = dto.EstimatedValue;
                }

                if (photos != null && photos.Count > 0)
                {
                    await HandlePhotos(item, photos);
                    LogChange(ItemHistoryType.ValueUpdated, "Photos update", $"{photos.Count} new photos added");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task HandlePhotos(Item item, List<IFormFile>? photos)
        {
            if (photos == null || photos.Count == 0) return;

            item.Photos ??= new List<ItemPhoto>();

            foreach (var photoFile in photos)
            {
                var result = await _photoService.AddPhotoAsync(photoFile);

                if (result.Error != null)
                    throw new Exception(result.Error.Message);

                var itemPhoto = new ItemPhoto
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    FilePath = result.SecureUrl.ToString(),
                    PublicId = result.PublicId,
                    UploadedAt = DateTime.UtcNow
                };

                await _context.ItemPhotos.AddAsync(itemPhoto);

                if (string.IsNullOrEmpty(item.PhotoUrl))
                {
                    item.PhotoUrl = itemPhoto.FilePath;
                    item.PublicId = itemPhoto.PublicId;
                }

                item.Photos.Add(itemPhoto);
            }
        }

        public async Task<bool> MoveItemAsync(Guid userId, Guid itemId, Guid? targetLocationId)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

            if (item == null)
                throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

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

            if (item == null)
                throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

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

            if (!exists)
                throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

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

            if (item == null || item.Sale == null)
                throw new KeyNotFoundException(LocalizationConstants.SALES.NOT_FOUND);

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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var item = await _context.Items
                    .Include(i => i.Photos)
                    .Include(i => i.Sale)
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

                if (item == null)
                    throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

                foreach (var photo in item.Photos)
                {
                    if (!string.IsNullOrEmpty(photo.PublicId))
                        await _photoService.DeletePhotoAsync(photo.PublicId);
                }

                var history = await _context.ItemHistories.Where(h => h.ItemId == itemId).ToListAsync();
                _context.ItemHistories.RemoveRange(history);

                if (item.Sale != null)
                {
                    _context.Sales.Remove(item.Sale);
                }

                _context.Items.Remove(item);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task BulkDeleteAsync(Guid userId, IEnumerable<Guid> itemIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var items = await _context.Items
                    .Include(i => i.Photos)
                    .Where(i => itemIds.Contains(i.Id) && i.UserId == userId)
                    .ToListAsync();

                if (!items.Any()) return;

                foreach (var item in items)
                {
                    foreach (var photo in item.Photos)
                    {
                        if (!string.IsNullOrEmpty(photo.PublicId))
                            await _photoService.DeletePhotoAsync(photo.PublicId);
                    }
                }

                _context.Items.RemoveRange(items);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}