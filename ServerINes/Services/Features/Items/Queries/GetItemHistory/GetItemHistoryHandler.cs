using INest.Constants;
using INest.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Items.Queries.GetItemHistory
{
    public class GetItemHistoryHandler : IRequestHandler<GetItemHistoryQuery, IEnumerable<ItemHistory>>
    {
        private readonly AppDbContext _context;

        public GetItemHistoryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemHistory>> Handle(GetItemHistoryQuery request, CancellationToken cancellationToken)
        {
            var exists = await _context.Items.AnyAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);
            if (!exists) throw new KeyNotFoundException(LocalizationConstants.ITEMS.ERRORS.NOT_FOUND);

            return await _context.ItemHistories
                .Where(h => h.ItemId == request.ItemId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
