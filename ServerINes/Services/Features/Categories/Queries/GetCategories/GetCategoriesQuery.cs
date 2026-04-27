using INest.Constants;
using INest.Models.Entities;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Categories.Queries.GetCategories
{
    public record GetCategoriesQuery(Guid UserId) : IRequest<IEnumerable<Category>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_CATEGORIES_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
