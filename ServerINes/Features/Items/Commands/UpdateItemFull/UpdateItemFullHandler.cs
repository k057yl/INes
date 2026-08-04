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

namespace INest.Features.Items.Commands.UpdateItemFull
{
    public class UpdateItemFullHandler : IRequestHandler<UpdateItemFullCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly ISanitizerService _sanitizer;
        private readonly ILogger<UpdateItemFullHandler> _logger;
        private readonly ICacheTracker _tracker;

        public UpdateItemFullHandler(
            AppDbContext context,
            IPhotoService photoService,
            ISanitizerService sanitizer,
            ILogger<UpdateItemFullHandler> logger,
            ICacheTracker tracker)
        {
            _context = context;
            _photoService = photoService;
            _sanitizer = sanitizer;
            _logger = logger;
            _tracker = tracker;
        }

        public async Task<bool> Handle(UpdateItemFullCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var safeName = _sanitizer.StripAllHtml(dto.Name);
            if (string.IsNullOrWhiteSpace(safeName)) throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

            var safeDesc = !string.IsNullOrEmpty(dto.Description) ? _sanitizer.SanitizeHtml(dto.Description) : null;

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var item = await _context.Items
                    .Include(i => i.Photos)
                    .Include(i => i.Details)
                    .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

                if (item == null) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

                if (item.Status != ItemStatus.Active)
                    throw new AppException(ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED);

                item.Name = safeName;
                item.Description = safeDesc;
                item.CategoryId = dto.CategoryId;
                item.MoveToLocation(dto.StorageLocationId);

                if (item.Details == null)
                {
                    item.Details = new ItemDetails
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        Currency = dto.Currency ?? "USD"
                    };
                }

                item.Details.PurchaseDate = dto.PurchaseDate;
                item.Details.PurchasePrice = dto.PurchasePrice;
                item.Details.Currency = dto.Currency ?? item.Details.Currency;
                item.Details.WarrantyExpiration = dto.WarrantyExpiration;

                if (dto.WarrantyExpiration.HasValue && dto.WarrantyExpiration != item.Details.WarrantyExpiration)
                {
                    item.Details.WarrantyAlertSent = false;
                }

                if (dto.ReceiptDocumentPath != null)
                {
                    item.Details.ReceiptDocumentPath = dto.ReceiptDocumentPath;
                }

                if (request.ReceiptFile != null)
                {
                    var receiptResult = await _photoService.AddReceiptAsync(request.ReceiptFile, request.UserId);
                    if (receiptResult.Error != null)
                        throw new AppException(ERRORS.IMAGE_PROCESSING_FAILED);

                    item.Details.ReceiptDocumentPath = receiptResult.SecureUrl.ToString();
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
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _tracker.InvalidateUserCache(request.UserId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Ошибка при полном обновлении предмета {ItemId}", request.ItemId);
                throw;
            }
        }

        private async Task HandlePhotosAsync(Item item, List<IFormFile>? photos, Guid userId, string? mainPhotoName = null)
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
                    _context.ItemPhotos.Add(itemPhoto);
                }
            }
        }
    }
}