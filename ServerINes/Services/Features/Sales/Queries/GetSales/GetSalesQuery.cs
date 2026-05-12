using INest.Constants;
using INest.Models.DTOs.Sale;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Sales.Queries.GetSales
{
    public record GetSalesQuery(Guid UserId, SaleFilterDto Filters) : IRequest<List<SaleResponseDto>>, ICacheableQuery
    {
        public string CacheKey => $"{CacheConstants.GET_SALES_HISTORY_KEY(UserId)}_" +
            $"S:{Filters.SearchQuery}_P:{Filters.PlatformId}_C:{Filters.CategoryId}_" +
            $"Sort:{(int)Filters.SortBy}_" +
            $"Pr:{Filters.MinPrice}-{Filters.MaxPrice}_Prof:{Filters.MinProfit}-{Filters.MaxProfit}_" +
            $"Dates:{Filters.StartDate?.Ticks}-{Filters.EndDate?.Ticks}";

        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
