using INest.Data.Enums;
using INest.Infrastructure.Storage;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Items.Commands.DeleteArchivedItemsBatch
{
    public class DeleteArchivedItemsBatchHandler : IRequestHandler<DeleteArchivedItemsBatchCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly ICacheTracker _tracker;

        public DeleteArchivedItemsBatchHandler(AppDbContext context, IPhotoService photoService, ICacheTracker tracker)
        {
            _context = context;
            _photoService = photoService;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteArchivedItemsBatchCommand request, CancellationToken cancellationToken)
        {
            if (request.ItemIds == null || !request.ItemIds.Any()) return false;

            var archivedItems = await _context.Items
                .Include(i => i.Photos)
                .Include(i => i.Details)
                .Where(i => i.UserId == request.UserId
                         && request.ItemIds.Contains(i.Id)
                         && i.Status == ItemStatus.Archived)
                .ToListAsync(cancellationToken);

            if (!archivedItems.Any()) return true;

            var allPhotos = archivedItems.SelectMany(i => i.Photos ?? new List<Data.Entities.Core.ItemPhoto>()).ToList();

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

            _context.Items.RemoveRange(archivedItems);

            await _context.SaveChangesAsync(cancellationToken);
            _tracker.InvalidateUserCache(request.UserId);

            return true;
        }
    }
}