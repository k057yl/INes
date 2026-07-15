using INest.Infrastructure.Storage;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Items.Commands.DeleteItemsBatch
{
    public class DeleteItemsBatchHandler : IRequestHandler<DeleteItemsBatchCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly ICacheTracker _tracker;

        public DeleteItemsBatchHandler(AppDbContext context, IPhotoService photoService, ICacheTracker tracker)
        {
            _context = context;
            _photoService = photoService;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteItemsBatchCommand request, CancellationToken cancellationToken)
        {
            if (request.ItemIds == null || !request.ItemIds.Any()) return false;

            var items = await _context.Items
                .Include(i => i.Photos)
                .Where(i => i.UserId == request.UserId && request.ItemIds.Contains(i.Id))
                .ToListAsync(cancellationToken);

            if (!items.Any()) return true;

            var allPhotos = items.SelectMany(i => i.Photos ?? new List<Data.Entities.Core.ItemPhoto>()).ToList();

            if (allPhotos.Any())
            {
                foreach (var photo in allPhotos)
                {
                    if (!string.IsNullOrEmpty(photo.PublicId))
                    {
                        await _photoService.DeletePhotoAsync(photo.PublicId);
                    }
                }

                _context.ItemPhotos.RemoveRange(allPhotos);
            }

            foreach (var item in items)
            {
                item.Archive();
            }

            await _context.SaveChangesAsync(cancellationToken);
            _tracker.InvalidateUserCache(request.UserId);

            return true;
        }
    }
}