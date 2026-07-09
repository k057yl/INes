using INest.Constants;
using INest.Features.Items.DTOs;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Items.Queries.GetItemById
{
    public record GetItemByIdQuery(Guid UserId, Guid ItemId) : IRequest<ItemDetailDto?>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_ITEM_HISTORY_KEY(UserId, ItemId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}