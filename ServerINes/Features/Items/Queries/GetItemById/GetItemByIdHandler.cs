using INest.Constants;
using INest.Features.Items.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Items.Queries.GetItemById
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
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .Include(i => i.History)
                .Include(i => i.Photos)
                .Include(i => i.Reminders)
                .Include(i => i.Details)
                .Where(i => i.UserId == request.UserId &&
                            i.Id == request.ItemId)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            var lending = await _context.Lendings
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.ItemId == item.Id, cancellationToken);

            var now = DateTime.UtcNow;

            bool isLendingOverdue =
                lending != null &&
                lending.ReturnedDate == null &&
                lending.ExpectedReturnDate.HasValue &&
                lending.ExpectedReturnDate.Value <= now;

            bool hasOverdueReminders =
                item.Reminders.Any(r => !r.IsCompleted && r.TriggerAt <= now);

            return new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Status = item.Status,

                Details = item.Details == null
                    ? null
                    : new ItemFinanceDto
                    {
                        PurchasePrice = item.Details.PurchasePrice,
                        EstimatedValue = item.Details.EstimatedValue,
                        Currency = item.Details.Currency ?? "USD",
                        PurchaseDate = item.Details.PurchaseDate,
                        WarrantyExpiration = item.Details.WarrantyExpiration
                    },

                PhotoUrl = item.PhotoUrl,

                StorageLocationId = item.StorageLocationId,
                StorageLocationName = item.StorageLocation?.Name,

                CategoryId = item.CategoryId,
                CategoryName = item.Category?.Name ?? SharedConstants.CATEGORY_NONE,

                IsLendingOverdue = isLendingOverdue,
                HasOverdueReminders = hasOverdueReminders,

                PersonName = lending?.PersonName,
                ContactEmail = lending?.ContactEmail,
                ExpectedReturnDate = lending?.ExpectedReturnDate,
                ReturnedDate = lending?.ReturnedDate,

                History = item.History.ToList(),
                Photos = item.Photos.ToList(),
                Reminders = item.Reminders.ToList()
            };
        }
    }
}