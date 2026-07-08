using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Commands.DeleteItem
{
    public class DeleteItemHandler : IRequestHandler<DeleteItemCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public DeleteItemHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            _context.Items.Remove(item);

            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}