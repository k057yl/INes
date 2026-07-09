using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Items.Queries.GetItemHistory
{
    public record GetItemHistoryQuery(Guid UserId, Guid ItemId) : IRequest<IEnumerable<ItemHistory>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_ITEM_HISTORY_KEY(UserId, ItemId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
