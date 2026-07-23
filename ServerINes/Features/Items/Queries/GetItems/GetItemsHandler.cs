using INest.Constants;
using INest.Data.Enums;
using INest.Features.Items.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Items.Queries.GetItems
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
                .Where(i => i.UserId == request.UserId)
                .AsNoTracking();

            if (filters.ShowArchived)
            {
                query = query.Where(i => i.Status == ItemStatus.Archived);
            }
            else if (!filters.IncludeArchived && !filters.Status.HasValue)
            {
                query = query.Where(i => i.Status != ItemStatus.Archived && i.Status != ItemStatus.Sold);
            }

            if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
            {
                var search = filters.SearchQuery.Trim();
                query = query.Where(i => EF.Functions.ILike(i.Name, $"%{search}%") ||
                                         (i.Description != null && EF.Functions.ILike(i.Description, $"%{search}%")));
            }

            if (filters.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filters.CategoryId);

            if (filters.StorageLocationId.HasValue)
                query = query.Where(i => i.StorageLocationId == filters.StorageLocationId);

            if (filters.Status.HasValue)
                query = query.Where(i => i.Status == filters.Status);

            if (filters.MinPrice.HasValue)
                query = query.Where(i => i.Details != null && i.Details.PurchasePrice >= filters.MinPrice);

            if (filters.MaxPrice.HasValue)
                query = query.Where(i => i.Details != null && i.Details.PurchasePrice <= filters.MaxPrice);

            query = filters.SortBy switch
            {
                ItemSortOption.NameAsc => query.OrderBy(i => i.Name),
                ItemSortOption.NameDesc => query.OrderByDescending(i => i.Name),
                ItemSortOption.PriceAsc => query.OrderBy(i => i.Details != null ? i.Details.PurchasePrice : 0),
                ItemSortOption.PriceDesc => query.OrderByDescending(i => i.Details != null ? i.Details.PurchasePrice : 0),
                ItemSortOption.Oldest => query.OrderBy(i => i.CreatedAt),
                _ => query.OrderByDescending(i => i.CreatedAt)
            };

            return await query
                .Select(item => new
                {
                    Item = item,
                    Lending = _context.Lendings.FirstOrDefault(l => l.ItemId == item.Id)
                })
                .Select(x => new ItemDto
                {
                    Id = x.Item.Id,
                    Name = x.Item.Name,
                    Description = x.Item.Description,
                    Status = x.Item.Status,
                    Details = x.Item.Details != null ? new ItemFinanceDto
                    {
                        PurchasePrice = x.Item.Details.PurchasePrice,
                        EstimatedValue = x.Item.Details.EstimatedValue,
                        Currency = x.Item.Details.Currency,
                        WarrantyExpiration = x.Item.Details.WarrantyExpiration,
                        ReceiptDocumentPath = x.Item.Details.ReceiptDocumentPath
                    } : null,

                    PhotoUrl = x.Item.PhotoUrl,
                    StorageLocationId = x.Item.StorageLocationId,
                    StorageLocationName = x.Item.StorageLocation != null ? x.Item.StorageLocation.Name : null,
                    CategoryId = x.Item.CategoryId,
                    CategoryName = x.Item.Category != null ? x.Item.Category.Name : SharedConstants.CATEGORY_NONE,
                    PersonName = x.Lending != null ? x.Lending.PersonName : null,
                    ContactEmail = x.Lending != null ? x.Lending.ContactEmail : null,
                    ExpectedReturnDate = x.Lending != null ? x.Lending.ExpectedReturnDate : null,
                    ReturnedDate = x.Lending != null ? x.Lending.ReturnedDate : null,

                    IsLendingOverdue = x.Lending != null && x.Lending.ReturnedDate == null && x.Lending.ExpectedReturnDate != null && x.Lending.ExpectedReturnDate <= now,
                    HasOverdueReminders = x.Item.Reminders.Any(r => !r.IsCompleted && r.TriggerAt <= now)
                })
                .ToListAsync(cancellationToken);
        }
    }
}