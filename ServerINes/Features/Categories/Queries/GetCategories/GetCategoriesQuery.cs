using INest.Constants;
using INest.Data.Entities.Core;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Categories.Queries.GetCategories
{
    public record GetCategoriesQuery(Guid UserId) : IRequest<IEnumerable<Category>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_CATEGORIES_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
