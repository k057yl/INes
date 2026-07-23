using INest.Data.Enums;
using INest.Exceptions;
using INest.Infrastructure.Storage;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Items.Commands.DeleteArchivedItem
{
    public class DeleteArchivedItemHandler : IRequestHandler<DeleteArchivedItemCommand>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly ICacheTracker _tracker;

        public DeleteArchivedItemHandler(AppDbContext context, IPhotoService photoService, ICacheTracker tracker)
        {
            _context = context;
            _photoService = photoService;
            _tracker = tracker;
        }

        public async Task Handle(DeleteArchivedItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .Include(i => i.Photos)
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            if (item.Status != ItemStatus.Archived)
            {
                throw new AppException(ITEMS.ERRORS.ONLY_ARCHIVED_CAN_BE_DELETED);
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
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
        }
    }
}