using INest.Services.Interfaces;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Items.Commands.DeleteItemsBatch
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

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var items = await _context.Items
                    .Include(i => i.Photos)
                    .Include(i => i.Sale)
                    .Include(i => i.Reminders)
                    .Where(i => i.UserId == request.UserId && request.ItemIds.Contains(i.Id))
                    .ToListAsync(cancellationToken);

                if (!items.Any()) return true;

                foreach (var item in items)
                {
                    foreach (var photo in item.Photos)
                    {
                        if (!string.IsNullOrEmpty(photo.PublicId))
                            await _photoService.DeletePhotoAsync(photo.PublicId);
                    }
                }

                var fetchedIds = items.Select(i => i.Id).ToList();

                var history = await _context.ItemHistories.Where(h => fetchedIds.Contains(h.ItemId)).ToListAsync(cancellationToken);
                _context.ItemHistories.RemoveRange(history);

                var sales = items.Where(i => i.Sale != null).Select(i => i.Sale!).ToList();
                if (sales.Any()) _context.Sales.RemoveRange(sales);

                var reminders = items.Where(i => i.Reminders != null).SelectMany(i => i.Reminders!).ToList();
                if (reminders.Any()) _context.Reminders.RemoveRange(reminders);

                _context.Items.RemoveRange(items);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _tracker.InvalidateUserCache(request.UserId);
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}