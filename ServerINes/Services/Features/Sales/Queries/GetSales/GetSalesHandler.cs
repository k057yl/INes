using INest.Models.DTOs.Sale;
using INest.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Sales.Queries.GetSales
{
    public class GetSalesHandler : IRequestHandler<GetSalesQuery, List<SaleResponseDto>>
    {
        private readonly AppDbContext _context;

        public GetSalesHandler(AppDbContext context) => _context = context;

        public async Task<List<SaleResponseDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
        {
            var f = request.Filters;
            var query = _context.Sales
                .Include(s => s.Platform)
                .Include(s => s.Category)
                .AsNoTracking()
                .Where(s => s.UserId == request.UserId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(f.SearchQuery))
                query = query.Where(s => s.ItemNameSnapshot.ToLower().Contains(f.SearchQuery.ToLower()));

            if (f.PlatformId.HasValue) query = query.Where(s => s.PlatformId == f.PlatformId);
            if (f.CategoryId.HasValue) query = query.Where(s => s.CategoryId == f.CategoryId);
            if (f.MinPrice.HasValue) query = query.Where(s => s.SalePrice >= f.MinPrice);
            if (f.MaxPrice.HasValue) query = query.Where(s => s.SalePrice <= f.MaxPrice);
            if (f.MinProfit.HasValue) query = query.Where(s => s.Profit >= f.MinProfit);
            if (f.MaxProfit.HasValue) query = query.Where(s => s.Profit <= f.MaxProfit);
            if (f.StartDate.HasValue) query = query.Where(s => s.SoldDate >= f.StartDate);
            if (f.EndDate.HasValue) query = query.Where(s => s.SoldDate <= f.EndDate);

            query = f.SortBy switch
            {
                SaleSortOption.DateAsc => query.OrderBy(s => s.SoldDate),
                SaleSortOption.PriceDesc => query.OrderByDescending(s => s.SalePrice),
                SaleSortOption.PriceAsc => query.OrderBy(s => s.SalePrice),
                SaleSortOption.ProfitDesc => query.OrderByDescending(s => s.Profit),
                _ => query.OrderByDescending(s => s.SoldDate)
            };

            return await query
                .Select(s => new SaleResponseDto
                {
                    SaleId = s.Id,
                    ItemId = s.ItemId ?? Guid.Empty,
                    ItemName = s.ItemNameSnapshot,
                    SalePrice = s.SalePrice,
                    Profit = s.Profit,
                    SoldDate = s.SoldDate,
                    PlatformName = s.Platform != null ? s.Platform.Name : null,
                    CategoryName = s.Category != null ? s.Category.Name : null
                })
                .ToListAsync(cancellationToken);
        }
    }
}