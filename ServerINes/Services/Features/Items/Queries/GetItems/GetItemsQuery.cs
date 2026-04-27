using INest.Constants;
using INest.Models.DTOs.Item;
using INest.Models.Entities;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Items.Queries.GetItems
{
    public record GetItemsQuery(Guid UserId, ItemFilterDto Filters) : IRequest<IEnumerable<Item>>, ICacheableQuery
    {
        public string CacheKey => $"{
            CacheConstants.GET_ITEMS_KEY(UserId)}_Cat:{Filters.CategoryId}_Loc:{Filters.StorageLocationId}_Stat:{Filters.Status}_Sort:{Filters.SortBy}_Min:{Filters.MinPrice}_Max:{Filters.MaxPrice}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
}