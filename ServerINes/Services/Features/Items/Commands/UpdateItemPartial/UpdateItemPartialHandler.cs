using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Commands.UpdateItemPartial
{
    public class UpdateItemPartialHandler : IRequestHandler<UpdateItemPartialCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ILogger<UpdateItemPartialHandler> _logger;
        private readonly ICacheTracker _tracker;

        public UpdateItemPartialHandler(
            AppDbContext context,
            IPhotoService photoService,
            IHtmlSanitizer sanitizer,
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
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);
            if (item.Status != ItemStatus.Active) throw new InvalidOperationException(ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                void LogChange(ItemHistoryType type, string? oldValue, string? newValue)
                {
                    if (oldValue == newValue) return;
                    _context.ItemHistories.Add(new ItemHistory
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        Type = type,
                        OldValue = oldValue,
                        NewValue = newValue,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (dto.Name != null)
                {
                    var safeName = _sanitizer.Sanitize(dto.Name);
                    if (string.IsNullOrWhiteSpace(safeName)) throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

                    if (safeName != item.Name)
                    {
                        LogChange(ItemHistoryType.ValueUpdated, item.Name, safeName);
                        item.Name = safeName;
                    }
                }

                if (dto.Description != null)
                {
                    var safeDesc = _sanitizer.Sanitize(dto.Description);
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

                    var oldStatus = item.Status;

                    if (targetLoc != null)
                    {
                        if (targetLoc.IsSalesLocation) item.Status = ItemStatus.Listed;
                        else if (targetLoc.IsLendingLocation) item.Status = ItemStatus.Lent;
                        else if (item.Status == ItemStatus.Listed || item.Status == ItemStatus.Lent)
                            item.Status = ItemStatus.Active;
                    }

                    if (oldStatus != item.Status)
                    {
                        LogChange(ItemHistoryType.StatusChanged, oldStatus.ToString(), item.Status.ToString());
                    }

                    item.StorageLocationId = targetLocationId;
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

                if (request.Photos != null && request.Photos.Count > 0)
                {
                    await HandlePhotosAsync(item, request.Photos);
                    LogChange(ItemHistoryType.ValueUpdated, null, $"{HISTORY.PHOTOS_ADDED_COUNT}|{request.Photos.Count}");
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _tracker.InvalidateUserCache(request.UserId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Ошибка при частичном обновлении предмета {ItemId}", request.ItemId);
                throw;
            }
        }

        private async Task HandlePhotosAsync(Item item, List<IFormFile>? photos)
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