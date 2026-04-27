using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Sales.Commands.DeleteSaleRecord
{
    public class DeleteSaleRecordHandler : IRequestHandler<DeleteSaleRecordCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public DeleteSaleRecordHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteSaleRecordCommand request, CancellationToken cancellationToken)
        {
            var sale = await _context.Sales
                .Include(s => s.Item)
                .FirstOrDefaultAsync(s => s.Id == request.SaleId && (s.Item == null || s.Item.UserId == request.UserId), cancellationToken);

            if (sale == null)
                throw new KeyNotFoundException(SALES.ERRORS.NOT_FOUND);

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}
