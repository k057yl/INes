using INest.Data.Entities.Core;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Storage;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Items.Commands.UpdateItemPartial
{
    public class UpdateItemPartialHandler : IRequestHandler<UpdateItemPartialCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly ISanitizerService _sanitizer;
        private readonly ILogger<UpdateItemPartialHandler> _logger;
        private readonly ICacheTracker _tracker;

        public UpdateItemPartialHandler(
            AppDbContext context,
            IPhotoService photoService,
            ISanitizerService sanitizer,
            ILogger<UpdateItemPartialHandler> logger,
            ICacheTracker tracker)
        {
            _context = context;
            _photoService = photoService;
            _sanitizer = sanitizer;
            _logger = logger;
            _tracker = tracker;
        }

        public async Task<bool> Handle(UpdateItemPartialCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var item = await _context.Items
                .Include(i => i.Photos)
                .Include(i => i.StorageLocation)
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            if (item.Status != ItemStatus.Active)
                throw new AppException(ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED);

            item.Details ??= new ItemDetails { Id = Guid.NewGuid(), ItemId = item.Id };

            try
            {
                void LogChange(ItemHistoryType historyType, string? oldValue, string? newValue)
                {
                    if (oldValue == newValue) return;
                    _context.ItemHistories.Add(new ItemHistory
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        UserId = request.UserId,
                        Type = historyType,
                        OldValue = oldValue,
                        NewValue = newValue,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (dto.Name != null)
                {
                    var safeName = _sanitizer.StripAllHtml(dto.Name);
                    if (string.IsNullOrWhiteSpace(safeName)) throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);
                    if (safeName != item.Name)
                    {
                        LogChange(ItemHistoryType.ValueUpdated, item.Name, safeName);
                        item.Name = safeName;
                    }
                }

                if (dto.Description != null)
                {
                    var safeDesc = _sanitizer.SanitizeHtml(dto.Description);
                    if (safeDesc != item.Description)
                    {
                        LogChange(ItemHistoryType.ValueUpdated, item.Description, safeDesc);
                        item.Description = safeDesc;
                    }
                }

                if (dto.CategoryId.HasValue && dto.CategoryId.Value != item.CategoryId)
                {
                    LogChange(ItemHistoryType.ValueUpdated, item.CategoryId.ToString(), dto.CategoryId.Value.ToString());
                    item.CategoryId = dto.CategoryId.Value;
                }

                if (dto.StorageLocationId.HasValue && dto.StorageLocationId.Value != item.StorageLocationId)
                {
                    var targetLocationId = dto.StorageLocationId.Value;
                    string? oldLocName = item.StorageLocation?.Name;

                    var targetLoc = await _context.StorageLocations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(l => l.Id == targetLocationId, cancellationToken);

                    LogChange(ItemHistoryType.Moved, oldLocName, targetLoc?.Name);
                    item.MoveToLocation(targetLocationId);
                }

                if (dto.PurchaseDate.HasValue && dto.PurchaseDate != item.Details.PurchaseDate)
                {
                    item.Details.PurchaseDate = dto.PurchaseDate;
                }

                if (dto.PurchasePrice.HasValue && dto.PurchasePrice != item.Details.PurchasePrice)
                {
                    LogChange(ItemHistoryType.ValueUpdated, item.Details.PurchasePrice?.ToString(), dto.PurchasePrice.Value.ToString());
                    item.Details.PurchasePrice = dto.PurchasePrice.Value;
                }

                if (dto.EstimatedValue.HasValue && dto.EstimatedValue != item.Details.EstimatedValue)
                {
                    LogChange(ItemHistoryType.ValueUpdated, item.Details.EstimatedValue?.ToString(), dto.EstimatedValue.Value.ToString());
                    item.Details.EstimatedValue = dto.EstimatedValue.Value;
                }

                if (dto.Currency != null)
                {
                    item.Details.Currency = dto.Currency;
                }

                if (dto.WarrantyExpiration.HasValue && dto.WarrantyExpiration != item.Details.WarrantyExpiration)
                {
                    item.Details.WarrantyExpiration = dto.WarrantyExpiration;
                    item.Details.WarrantyAlertSent = false;
                }

                if (dto.Reminder != null && dto.Reminder.TriggerAt != DateTime.MinValue)
                {
                    var safeReminderTitle = string.IsNullOrWhiteSpace(dto.Reminder.Title)
                        ? REMINDERS.CUSTOM
                        : _sanitizer.StripAllHtml(dto.Reminder.Title);

                    var reminder = new Reminder
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.UserId,
                        ItemId = item.Id,
                        Title = safeReminderTitle,
                        Type = dto.Reminder.Type,
                        Recurrence = dto.Reminder.Recurrence,
                        TriggerAt = dto.Reminder.TriggerAt.ToUniversalTime(),
                        SendNotification = dto.Reminder.SendNotification,
                        SendTelegram = dto.Reminder.SendTelegram,
                        IsCompleted = false,
                        IsNotificationSent = false
                    };

                    _context.Reminders.Add(reminder);
                }

                if (request.Photos != null && request.Photos.Count > 0)
                {
                    await HandlePhotosAsync(item, request.Photos, request.UserId);
                    LogChange(ItemHistoryType.ValueUpdated, null, $"{HISTORY.PHOTOS_ADDED_COUNT}|{request.Photos.Count}");
                }

                await _context.SaveChangesAsync(cancellationToken);
                _tracker.InvalidateUserCache(request.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при частичном обновлении предмета {ItemId}", request.ItemId);
                throw;
            }
        }

        private async Task HandlePhotosAsync(Item item, List<IFormFile>? photos, Guid userId)
        {
            if (photos == null || photos.Count == 0) return;

            item.Photos ??= new List<ItemPhoto>();

            var uploadTasks = photos.Select(async photoFile =>
            {
                var result = await _photoService.AddPhotoAsync(photoFile, userId);
                return new { File = photoFile, Result = result };
            }).ToList();

            var uploadResults = await Task.WhenAll(uploadTasks);

            foreach (var upload in uploadResults)
            {
                if (upload.Result.Error != null)
                    throw new AppException(ERRORS.IMAGE_PROCESSING_FAILED);

                var itemPhoto = new ItemPhoto
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    UserId = item.UserId,
                    FilePath = upload.Result.SecureUrl.ToString(),
                    PublicId = upload.Result.PublicId
                };

                if (string.IsNullOrEmpty(item.PhotoUrl))
                {
                    item.PhotoUrl = itemPhoto.FilePath;
                    item.PublicId = itemPhoto.PublicId;
                }

                item.Photos.Add(itemPhoto);

                if (_context.Entry(item).State != EntityState.Detached)
                {
                    _context.ItemPhotos.Add(itemPhoto);
                }
            }
        }
    }
}