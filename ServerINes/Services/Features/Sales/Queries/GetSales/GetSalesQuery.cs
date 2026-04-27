using INest.Constants;
using INest.Models.DTOs.Sale;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Sales.Queries.GetSales
{
    public record GetSalesQuery(Guid UserId) : IRequest<List<SaleResponseDto>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_SALES_HISTORY_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
