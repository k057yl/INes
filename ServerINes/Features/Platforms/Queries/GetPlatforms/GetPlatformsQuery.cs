using INest.Constants;
using INest.Data.Entities.Finances;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Platforms.Queries.GetPlatforms
{
    public record GetPlatformsQuery(Guid UserId) : IRequest<IEnumerable<Platform>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_PLATFORMS_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
