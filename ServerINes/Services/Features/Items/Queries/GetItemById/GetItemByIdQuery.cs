using INest.Constants;
using INest.Models.DTOs.Item;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Items.Queries.GetItemById
{
    public record GetItemByIdQuery(Guid UserId, Guid ItemId) : IRequest<ItemDetailDto?>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_ITEM_HISTORY_KEY(UserId, ItemId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}