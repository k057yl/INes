using INest.Models.DTOs.Item;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Items.Queries.GetItemById
{
    public class GetItemByIdHandler : IRequestHandler<GetItemByIdQuery, ItemDetailDto?>
    {
        private readonly AppDbContext _context;

        public GetItemByIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ItemDetailDto?> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .Include(i => i.History)
                .Include(i => i.Photos)
                .Include(i => i.Reminders)
                .Where(i => i.UserId == request.UserId && i.Id == request.ItemId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            var lending = await _context.Lendings
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.ItemId == item.Id, cancellationToken);

            var now = DateTime.UtcNow;

            bool isLendingOverdue = lending != null &&
                                    lending.ReturnedDate == null &&
                                    lending.ExpectedReturnDate.HasValue &&
                                    lending.ExpectedReturnDate.Value <= now;

            bool hasOverdueReminders = item.Reminders.Any(r => !r.IsCompleted && r.TriggerAt <= now);

            return new ItemDetailDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Status = item.Status,
                PurchasePrice = item.PurchasePrice,
                EstimatedValue = item.EstimatedValue,
                Currency = item.Currency,
                PhotoUrl = item.PhotoUrl,
                StorageLocationId = item.StorageLocationId,
                StorageLocation = item.StorageLocation,
                CategoryId = item.CategoryId,
                Category = item.Category,
                History = item.History.ToList(),
                Photos = item.Photos.ToList(),
                Reminders = item.Reminders.ToList(),
                Lending = lending,
                IsLendingOverdue = isLendingOverdue,
                HasOverdueReminders = hasOverdueReminders
            };
        }
    }
}