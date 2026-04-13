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
        private readonly IEmailService _emailService;

        public ItemService(AppDbContext context, IPhotoService photoService, IEmailService emailService)
        {
            _context = context;
            _photoService = photoService;
            _emailService = emailService;
        }

        private void AddHistoryEntry(Guid itemId, ItemHistoryType type, string? oldValue = null, string? newValue = null, string? comment = null)
        {
            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                Type = type,
                OldValue = oldValue,
                NewValue = newValue,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            });
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
                    EstimatedValue = dto.EstimatedValue ?? dto.PurchasePrice,
                    Currency = dto.Currency ?? "USD",
                    CreatedAt = DateTime.UtcNow,
                    Photos = new List<ItemPhoto>()
                };

                _context.Items.Add(item);

                AddHistoryEntry(item.Id, ItemHistoryType.Created, null, item.Name);

                if (dto.Status == ItemStatus.Lent || dto.Status == ItemStatus.Borrowed)
                {
                    item.Lending = new Lending
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        PersonName = dto.PersonName ?? "Unknown",
                        ContactEmail = dto.ContactEmail,
                        ExpectedReturnDate = dto.ExpectedReturnDate,
                        DateGiven = DateTime.UtcNow,
                        Direction = dto.Status == ItemStatus.Borrowed ? LendingDirection.In : LendingDirection.Out,
                        SendNotification = dto.SendNotification,
                        ValueAtLending = item.EstimatedValue
                    };

                    AddHistoryEntry(item.Id,
                        dto.Status == ItemStatus.Lent ? ItemHistoryType.Lent : ItemHistoryType.Borrowed,
                        null,
                        item.Lending.PersonName);

                    if (dto.SendNotification && dto.ExpectedReturnDate.HasValue)
                    {
                        var reminderDate = dto.ExpectedReturnDate.Value.AddDays(-1);
                        if (reminderDate > DateTime.UtcNow)
                        {
                            var reminder = new Reminder
                            {
                                Id = Guid.NewGuid(),
                                ItemId = item.Id,
                                TriggerAt = reminderDate,
                                Type = ReminderType.ReturnItem,
                                IsCompleted = false
                            };
                            _context.Reminders.Add(reminder);

                            AddHistoryEntry(item.Id, ItemHistoryType.ReminderScheduled, null, reminderDate.ToString("dd.MM.yyyy"));
                        }
                    }

                    if (dto.SendNotification && !string.IsNullOrEmpty(dto.ContactEmail))
                    {
                        _ = _emailService.SendLendingNotificationAsync(
                            dto.ContactEmail,
                            item.Name,
                            item.Lending.PersonName,
                            item.Lending.ExpectedReturnDate,
                            dto.Status == ItemStatus.Borrowed);
                    }
                }

                if (photos != null && photos.Count > 0)
                {
                    await HandlePhotos(item, photos, dto.MainPhotoName);
                }

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

        private async Task HandlePhotos(Item item, List<IFormFile>? photos, string? mainPhotoName = null)
        {
            if (photos == null || photos.Count == 0) return;

            item.Photos ??= new List<ItemPhoto>();

            var uploadTasks = photos.Select(async photoFile =>
            {
                var result = await _photoService.AddPhotoAsync(photoFile);
                return new { File = photoFile, Result = result };
            }).ToList();

            var uploadResults = await Task.WhenAll(uploadTasks);

            foreach (var upload in uploadResults)
            {
                if (upload.Result.Error != null)
                    throw new Exception(upload.Result.Error.Message);

                var itemPhoto = new ItemPhoto
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    FilePath = upload.Result.SecureUrl.ToString(),
                    PublicId = upload.Result.PublicId,
                    UploadedAt = DateTime.UtcNow
                };

                if ((!string.IsNullOrEmpty(mainPhotoName) && upload.File.FileName == mainPhotoName) ||
                    string.IsNullOrEmpty(item.PhotoUrl))
                {
                    item.PhotoUrl = itemPhoto.FilePath;
                    item.PublicId = itemPhoto.PublicId;
                }

                item.Photos.Add(itemPhoto);
                if (_context.Entry(item).State != EntityState.Detached)
                {
                    await _context.ItemPhotos.AddAsync(itemPhoto);
                }
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
                .Include(i => i.Lending)
                .Include(i => i.Reminders)
                .Include(i => i.History.OrderByDescending(h => h.CreatedAt))
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
                item.Currency = dto.Currency ?? item.Currency;

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
                    throw new InvalidOperationException(LocalizationConstants.ITEMS.ONLY_ACTIVE_CAN_BE_EDITED);

                void LogChange(ItemHistoryType type, string? oldValue, string? newValue)
                {
                    if (oldValue == newValue) return;
                    AddHistoryEntry(item.Id, type, oldValue, newValue);
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
                    await MoveItemAsync(userId, itemId, dto.StorageLocationId.Value);
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

                if (dto.Currency != null)
                {
                    item.Currency = dto.Currency;
                }

                if (photos != null && photos.Count > 0)
                {
                    await HandlePhotos(item, photos);
                    LogChange(ItemHistoryType.ValueUpdated, null, $"{LocalizationConstants.HISTORY.PHOTOS_ADDED_COUNT}|{photos.Count}");
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

        public async Task<bool> MoveItemAsync(Guid userId, Guid itemId, Guid? targetLocationId)
        {
            var item = await _context.Items
                .Include(i => i.StorageLocation)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

            if (item == null) throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

            if (item.StorageLocationId != targetLocationId)
            {
                string? oldLocName = item.StorageLocation?.Name;
                string? newLocName = null;

                if (targetLocationId.HasValue)
                {
                    var targetLoc = await _context.StorageLocations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == targetLocationId.Value);
                    newLocName = targetLoc?.Name;
                }

                AddHistoryEntry(item.Id, ItemHistoryType.Moved, oldLocName, newLocName);

                var oldStatus = item.Status;

                if (targetLocationId.HasValue)
                {
                    var targetLocation = await _context.StorageLocations.FirstOrDefaultAsync(l => l.Id == targetLocationId.Value);
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
                    AddHistoryEntry(item.Id, ItemHistoryType.StatusChanged, oldStatus.ToString(), item.Status.ToString());
                }

                item.StorageLocationId = targetLocationId;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> ChangeStatusAsync(Guid userId, Guid itemId, ItemStatus newStatus)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);
            if (item == null) throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

            if (item.Status != newStatus)
            {
                ItemHistoryType type = ItemHistoryType.StatusChanged;
                if (newStatus == ItemStatus.Sold) type = ItemHistoryType.Sold;

                AddHistoryEntry(item.Id, type, item.Status.ToString(), newStatus.ToString());
                item.Status = newStatus;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<IEnumerable<ItemHistory>> GetItemHistoryAsync(Guid userId, Guid itemId)
        {
            var exists = await _context.Items.AnyAsync(i => i.Id == itemId && i.UserId == userId);
            if (!exists) throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

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

            if (item == null || item.Sale == null) throw new KeyNotFoundException(LocalizationConstants.SALES.NOT_FOUND);

            _context.Sales.Remove(item.Sale);
            item.Status = ItemStatus.Active;

            AddHistoryEntry(item.Id, ItemHistoryType.Returned, SharedConstants.OLD_VALUE, SharedConstants.NEW_VALUE, "Sale canceled");

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
                    .Include(i => i.Reminders)
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

                if (item == null) throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

                foreach (var photo in item.Photos)
                {
                    if (!string.IsNullOrEmpty(photo.PublicId))
                        await _photoService.DeletePhotoAsync(photo.PublicId);
                }

                var history = await _context.ItemHistories.Where(h => h.ItemId == itemId).ToListAsync();
                _context.ItemHistories.RemoveRange(history);

                if (item.Sale != null) _context.Sales.Remove(item.Sale);
                if (item.Reminders != null && item.Reminders.Any()) _context.Reminders.RemoveRange(item.Reminders);

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
    }
}