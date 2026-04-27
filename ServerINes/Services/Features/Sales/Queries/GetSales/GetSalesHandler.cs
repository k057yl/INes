using INest.Models.DTOs.Sale;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Sales.Queries.GetSales
{
    public class GetSalesHandler : IRequestHandler<GetSalesQuery, List<SaleResponseDto>>
    {
        private readonly AppDbContext _context;

        public GetSalesHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SaleResponseDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Sales
                .Include(s => s.Platform)
                .AsNoTracking()
                .Where(s => s.UserId == request.UserId)
                .OrderByDescending(s => s.SoldDate)
                .Select(s => new SaleResponseDto
                {
                    SaleId = s.Id,
                    ItemId = s.ItemId ?? Guid.Empty,
                    ItemName = s.ItemNameSnapshot,
                    SalePrice = s.SalePrice,
                    Profit = s.Profit,
                    SoldDate = s.SoldDate,
                    PlatformName = s.Platform != null ? s.Platform.Name : null
                })
                .ToListAsync(cancellationToken);
        }
    }
}