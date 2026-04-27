using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Commands.CreateItem
{
    public class CreateItemHandler : IRequestHandler<CreateItemCommand, Item>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly ILendingService _lendingService;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ILogger<CreateItemHandler> _logger;
        private readonly ICacheTracker _tracker;

        public CreateItemHandler(
            AppDbContext context,
            IPhotoService photoService,
            ILendingService lendingService,
            IHtmlSanitizer sanitizer,
            ILogger<CreateItemHandler> logger,
            ICacheTracker tracker)
        {
            _context = context;
            _photoService = photoService;
            _lendingService = lendingService;
            _sanitizer = sanitizer;
            _logger = logger;
            _tracker = tracker;
        }

        public async Task<Item> Handle(CreateItemCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var safeName = _sanitizer.Sanitize(dto.Name);
            if (string.IsNullOrWhiteSpace(safeName)) throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

            var safeDesc = !string.IsNullOrEmpty(dto.Description) ? _sanitizer.Sanitize(dto.Description) : null;
            var safePerson = !string.IsNullOrEmpty(dto.PersonName) ? _sanitizer.Sanitize(dto.PersonName) : "Unknown";

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Name = safeName,
                    Description = safeDesc,
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

                _context.ItemHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    Type = ItemHistoryType.Created,
                    NewValue = item.Name,
                    CreatedAt = DateTime.UtcNow
                });

                await _lendingService.SyncLendingStateAsync(
                    item, dto.Status, safePerson, dto.ContactEmail, dto.ExpectedReturnDate, dto.SendNotification);

                if (request.Photos != null && request.Photos.Count > 0)
                {
                    await HandlePhotosAsync(item, request.Photos, dto.MainPhotoName);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _tracker.InvalidateUserCache(request.UserId);
                return item;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Критическая ошибка при создании предмета");
                throw;
            }
        }

        private async Task HandlePhotosAsync(Item item, List<IFormFile> photos, string? mainPhotoName = null)
        {
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