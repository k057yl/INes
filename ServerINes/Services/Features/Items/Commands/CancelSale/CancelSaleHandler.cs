using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Models.Enums;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Commands.CancelSale
{
    public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public CancelSaleHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.ItemId == item.Id && s.UserId == request.UserId, cancellationToken);

            if (sale == null) throw new KeyNotFoundException(SALES.ERRORS.NOT_FOUND);

            _context.Sales.Remove(sale);
            item.Status = ItemStatus.Active;

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = request.UserId,
                Type = ItemHistoryType.Returned,
                OldValue = SharedConstants.OLD_VALUE,
                NewValue = SharedConstants.NEW_VALUE,
                Comment = HISTORY.SALES_CANCELED
            });

            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}