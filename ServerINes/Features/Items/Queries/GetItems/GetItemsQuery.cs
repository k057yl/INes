using INest.Constants;
using INest.Features.Items.DTOs;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Items.Queries.GetItems
{
    public record GetItemsQuery(Guid UserId, ItemFilterDto Filters) : IRequest<IEnumerable<ItemDto>>, ICacheableQuery
    {
        public string CacheKey => $"{CacheConstants.GET_ITEMS_KEY(UserId)}_Search:{Filters.SearchQuery}_Cat:{Filters.CategoryId}_Loc:{Filters.StorageLocationId}_Stat:{Filters.Status}_Sort:{Filters.SortBy}_Min:{Filters.MinPrice}_Max:{Filters.MaxPrice}";

        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
}