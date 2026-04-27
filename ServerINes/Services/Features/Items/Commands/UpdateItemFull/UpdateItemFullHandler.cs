using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Commands.UpdateItemFull
{
    public class UpdateItemFullHandler : IRequestHandler<UpdateItemFullCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ILogger<UpdateItemFullHandler> _logger;
        private readonly ICacheTracker _tracker;

        public UpdateItemFullHandler(
            AppDbContext context,
            IPhotoService photoService,
            IHtmlSanitizer sanitizer,
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

            var safeName = _sanitizer.Sanitize(dto.Name);
            if (string.IsNullOrWhiteSpace(safeName)) throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

            var safeDesc = !string.IsNullOrEmpty(dto.Description) ? _sanitizer.Sanitize(dto.Description) : null;

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var item = await _context.Items
                    .Include(i => i.Photos)
                    .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

                if (item == null) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

                if (item.Status != ItemStatus.Active)
                    throw new InvalidOperationException(ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED);

                item.Name = safeName;
                item.Description = safeDesc;
                item.CategoryId = dto.CategoryId;
                item.StorageLocationId = dto.StorageLocationId;
                item.Status = dto.Status;
                item.PurchaseDate = dto.PurchaseDate;
                item.PurchasePrice = dto.PurchasePrice;
                item.EstimatedValue = dto.EstimatedValue;
                item.Currency = dto.Currency ?? item.Currency;

                if (request.Photos != null && request.Photos.Count > 0)
                {
                    await HandlePhotosAsync(item, request.Photos);
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

        private async Task HandlePhotosAsync(Item item, List<IFormFile>? photos, string? mainPhotoName = null)
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
                    _context.ItemPhotos.Add(itemPhoto);
                }
            }
        }
    }
}