using INest.Data.Enums;
using INest.Infrastructure.Storage;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Items.Commands.DeleteItem
{
    public class DeleteItemHandler : IRequestHandler<DeleteItemCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly ICacheTracker _tracker;

        public DeleteItemHandler(AppDbContext context, IPhotoService photoService, ICacheTracker tracker)
        {
            _context = context;
            _photoService = photoService;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .Include(i => i.Photos)
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            if (item.Status != ItemStatus.Active && item.Status != ItemStatus.Archived)
            {
                throw new InvalidOperationException(ITEMS.ERRORS.CANNOT_ARCHIVE_NON_ACTIVE);
            }

            if (item.Photos != null && item.Photos.Any())
            {
                foreach (var photo in item.Photos)
                {
                    if (!string.IsNullOrEmpty(photo.PublicId))
                    {
                        await _photoService.DeletePhotoAsync(photo.PublicId);
                    }
                }

                _context.ItemPhotos.RemoveRange(item.Photos);
            }

            if (item.Details != null && !string.IsNullOrEmpty(item.Details.ReceiptDocumentPath))
            {
                item.Details.ReceiptDocumentPath = null;
            }

            item.Archive();

            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}