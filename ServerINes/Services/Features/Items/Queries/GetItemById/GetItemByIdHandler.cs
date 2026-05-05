using INest.Models.DTOs.Item;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Queries.GetItemById
{
    public class GetItemByIdHandler : IRequestHandler<GetItemByIdQuery, ItemDto?>
    {
        private readonly AppDbContext _context;

        public GetItemByIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ItemDto?> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .Where(i => i.UserId == request.UserId && i.Id == request.ItemId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            return new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                IsOverdue =
                    item.Lending != null &&
                    item.Lending.ReturnedDate == null &&
                    item.Lending.ExpectedReturnDate.HasValue &&
                    item.Lending.ExpectedReturnDate <= DateTime.UtcNow
            };
        }
    }
}