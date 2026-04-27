using INest.Constants;
using INest.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Items.Queries.GetItemById
{
    public class GetItemByIdHandler : IRequestHandler<GetItemByIdQuery, Item?>
    {
        private readonly AppDbContext _context;

        public GetItemByIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Item?> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .Where(i => i.UserId == request.UserId && i.Id == request.ItemId)
                .Include(i => i.Photos)
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .Include(i => i.Sale)
                .Include(i => i.Lending)
                .Include(i => i.Reminders)
                .Include(i => i.History.OrderByDescending(h => h.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(LocalizationConstants.ITEMS.ERRORS.NOT_FOUND);

            return item;
        }
    }
}