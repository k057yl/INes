using INest.Constants;
using INest.Models.Entities;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Platforms.Queries.GetPlatforms
{
    public record GetPlatformsQuery(Guid UserId) : IRequest<IEnumerable<Platform>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_PLATFORMS_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
