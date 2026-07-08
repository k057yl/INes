using INest.Constants;
using INest.Models.DTOs.Item;
using INest.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Items.Queries.GetItems
{
    public class GetItemsHandler : IRequestHandler<GetItemsQuery, IEnumerable<ItemDto>>
    {
        private readonly AppDbContext _context;

        public GetItemsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemDto>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
        {
            var filters = request.Filters;
            var now = DateTime.UtcNow;

            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .Where(i => i.UserId == request.UserId)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
            {
                var search = filters.SearchQuery.Trim().ToLower();
                query = query.Where(i =>
                    i.Name.ToLower().Contains(search) ||
                    (i.Description != null && i.Description.ToLower().Contains(search)));
            }

            if (filters.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filters.CategoryId);

            if (filters.StorageLocationId.HasValue)
                query = query.Where(i => i.StorageLocationId == filters.StorageLocationId);

            if (filters.Status.HasValue)
                query = query.Where(i => i.Status == filters.Status);

            if (filters.MinPrice.HasValue)
                query = query.Where(i => i.PurchasePrice >= filters.MinPrice);

            if (filters.MaxPrice.HasValue)
                query = query.Where(i => i.PurchasePrice <= filters.MaxPrice);

            query = filters.SortBy switch
            {
                ItemSortOption.NameAsc => query.OrderBy(i => i.Name),
                ItemSortOption.NameDesc => query.OrderByDescending(i => i.Name),
                ItemSortOption.PriceAsc => query.OrderBy(i => i.PurchasePrice),
                ItemSortOption.PriceDesc => query.OrderByDescending(i => i.PurchasePrice),
                ItemSortOption.Oldest => query.OrderBy(i => i.CreatedAt),
                _ => query.OrderByDescending(i => i.CreatedAt)
            };

            return await query
                .Select(item => new ItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Status = item.Status,
                    EstimatedValue = item.EstimatedValue,
                    Currency = item.Currency,
                    PhotoUrl = item.PhotoUrl,
                    StorageLocationId = item.StorageLocationId,
                    StorageLocationName = item.StorageLocation != null ? item.StorageLocation.Name : null,
                    CategoryId = item.CategoryId,
                    CategoryName = item.Category != null ? item.Category.Name : SharedConstants.CATEGORY_NONE,

                    PersonName = _context.Lendings.Where(l => l.ItemId == item.Id).Select(l => l.PersonName).FirstOrDefault(),
                    ContactEmail = _context.Lendings.Where(l => l.ItemId == item.Id).Select(l => l.ContactEmail).FirstOrDefault(),
                    ExpectedReturnDate = _context.Lendings.Where(l => l.ItemId == item.Id).Select(l => l.ExpectedReturnDate).FirstOrDefault(),
                    ReturnedDate = _context.Lendings.Where(l => l.ItemId == item.Id).Select(l => l.ReturnedDate).FirstOrDefault(),

                    IsLendingOverdue = _context.Lendings.Any(l =>
                        l.ItemId == item.Id &&
                        l.ReturnedDate == null &&
                        l.ExpectedReturnDate != null &&
                        l.ExpectedReturnDate <= now),

                    HasOverdueReminders = _context.Reminders.Any(r =>
                        r.ItemId == item.Id &&
                        !r.IsCompleted &&
                        r.TriggerAt <= now)
                })
                .ToListAsync(cancellationToken);
        }
    }
}