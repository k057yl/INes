using INest.Models.Entities;
using INest.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Items.Queries.GetItems
{
    public class GetItemsHandler : IRequestHandler<GetItemsQuery, IEnumerable<Item>>
    {
        private readonly AppDbContext _context;

        public GetItemsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
        {
            var filters = request.Filters;
            var query = _context.Items
                .Where(i => i.UserId == request.UserId)
                .Include(i => i.Photos)
                .Include(i => i.Category)
                .Include(i => i.StorageLocation)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
            {
                var search = filters.SearchQuery.Trim().ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(search) ||
                                         (i.Description != null && i.Description.ToLower().Contains(search)));
            }

            if (filters.CategoryId.HasValue) query = query.Where(i => i.CategoryId == filters.CategoryId);
            if (filters.StorageLocationId.HasValue) query = query.Where(i => i.StorageLocationId == filters.StorageLocationId.Value);
            if (filters.Status.HasValue) query = query.Where(i => i.Status == filters.Status);
            if (filters.MinPrice.HasValue) query = query.Where(i => i.PurchasePrice >= filters.MinPrice);
            if (filters.MaxPrice.HasValue) query = query.Where(i => i.PurchasePrice <= filters.MaxPrice);

            query = filters.SortBy switch
            {
                ItemSortOption.NameAsc => query.OrderBy(i => i.Name),
                ItemSortOption.NameDesc => query.OrderByDescending(i => i.Name),
                ItemSortOption.PriceAsc => query.OrderBy(i => i.PurchasePrice),
                ItemSortOption.PriceDesc => query.OrderByDescending(i => i.PurchasePrice),
                ItemSortOption.Oldest => query.OrderBy(i => i.CreatedAt),
                _ => query.OrderByDescending(i => i.CreatedAt)
            };

            return await query.ToListAsync(cancellationToken);
        }
    }
}