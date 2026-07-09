using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Items.Commands.DeleteItemsBatch
{
    public class DeleteItemsBatchHandler : IRequestHandler<DeleteItemsBatchCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public DeleteItemsBatchHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteItemsBatchCommand request, CancellationToken cancellationToken)
        {
            if (request.ItemIds == null || !request.ItemIds.Any()) return false;

            var items = await _context.Items
                .Where(i => i.UserId == request.UserId && request.ItemIds.Contains(i.Id))
                .ToListAsync(cancellationToken);

            if (!items.Any()) return true;

            _context.Items.RemoveRange(items);

            await _context.SaveChangesAsync(cancellationToken);
            _tracker.InvalidateUserCache(request.UserId);

            return true;
        }
    }
}