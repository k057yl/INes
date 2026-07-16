using Ganss.Xss;
using INest.Data.Entities.Core;
using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Infrastructure.Storage;
using INest.Infrastructure.Tracker;
using MediatR;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Items.Commands.CreateItem
{
    public class CreateItemHandler : IRequestHandler<CreateItemCommand, Item>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly LendingService _lendingService;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ILogger<CreateItemHandler> _logger;
        private readonly ICacheTracker _tracker;

        public CreateItemHandler(
            AppDbContext context,
            IPhotoService photoService,
            LendingService lendingService,
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
            if (string.IsNullOrWhiteSpace(safeName))
                throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED);

            var safeDescription = string.IsNullOrWhiteSpace(dto.Description) ? null : _sanitizer.Sanitize(dto.Description);
            var safePerson = string.IsNullOrWhiteSpace(dto.PersonName) ? "Unknown" : _sanitizer.Sanitize(dto.PersonName);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Name = safeName,
                    Description = safeDescription,
                    CategoryId = dto.CategoryId,
                    StorageLocationId = dto.StorageLocationId,
                    PurchaseDate = dto.PurchaseDate,
                    PurchasePrice = dto.PurchasePrice,
                    EstimatedValue = dto.EstimatedValue ?? dto.PurchasePrice,
                    Currency = dto.Currency ?? "USD",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Items.Add(item);

                _context.ItemHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    UserId = request.UserId,
                    Type = ItemHistoryType.Created,
                    NewValue = item.Name,
                    CreatedAt = DateTime.UtcNow
                });

                if (request.Photos.Count > 0)
                {
                    await HandlePhotosAsync(item, request.Photos, dto.MainPhotoName);
                }

                if (dto.Status == ItemStatus.Lent)
                {
                    await _lendingService.LendAsync(
                        item,
                        safePerson,
                        dto.ContactEmail,
                        dto.ExpectedReturnDate,
                        dto.SendNotification);
                }
                else if (dto.Status == ItemStatus.Borrowed)
                {
                    item.Borrow();

                    var lending = new Lending
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        UserId = request.UserId,
                        PersonName = safePerson,
                        DateGiven = DateTime.UtcNow,
                        ExpectedReturnDate = dto.ExpectedReturnDate,
                        ValueAtLending = item.EstimatedValue,
                        Comment = safeDescription
                    };

                    _context.Lendings.Add(lending);
                }
                else if (dto.Status == ItemStatus.Sold)
                {
                    item.Sell();
                }
                else if (dto.Status != ItemStatus.Active)
                {
                    throw new AppException(ITEMS.ERRORS.INVALID_INITIAL_STATUS);
                }

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                _tracker.InvalidateUserCache(request.UserId);

                return item;
            }
            catch (Exception ex)
            {
                if (_context.Database.CurrentTransaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                _logger.LogError(ex, "Error while creating item.");
                throw;
            }
        }

        private async Task HandlePhotosAsync(Item item, List<IFormFile> photos, string? mainPhotoName)
        {
            var uploadTasks = photos.Select(async photo =>
            {
                var result = await _photoService.AddPhotoAsync(photo);
                return new { File = photo, Result = result };
            });

            var uploads = await Task.WhenAll(uploadTasks);

            foreach (var upload in uploads)
            {
                if (upload.Result.Error != null)
                    throw new AppException(upload.Result.Error.Message);

                var photo = new ItemPhoto
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    FilePath = upload.Result.SecureUrl.ToString(),
                    PublicId = upload.Result.PublicId,
                    UploadedAt = DateTime.UtcNow
                };

                if ((!string.IsNullOrWhiteSpace(mainPhotoName) && upload.File.FileName == mainPhotoName) ||
                    string.IsNullOrEmpty(item.PhotoUrl))
                {
                    item.PhotoUrl = photo.FilePath;
                    item.PublicId = photo.PublicId;
                }

                item.Photos.Add(photo);
                _context.ItemPhotos.Add(photo);
            }
        }
    }
}