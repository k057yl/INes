using INest.Services.Interfaces;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Commands.DeleteItem
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
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var item = await _context.Items
                    .Include(i => i.Photos)
                    .Include(i => i.Sale)
                    .Include(i => i.Reminders)
                    .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

                if (item == null) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

                foreach (var photo in item.Photos)
                {
                    if (!string.IsNullOrEmpty(photo.PublicId))
                        await _photoService.DeletePhotoAsync(photo.PublicId);
                }

                var history = await _context.ItemHistories.Where(h => h.ItemId == request.ItemId).ToListAsync(cancellationToken);
                _context.ItemHistories.RemoveRange(history);

                if (item.Sale != null) _context.Sales.Remove(item.Sale);
                if (item.Reminders != null && item.Reminders.Any()) _context.Reminders.RemoveRange(item.Reminders);

                _context.Items.Remove(item);
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