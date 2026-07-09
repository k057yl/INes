using INest.Constants;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Locations.Queries.GetLocations
{
    public record GetLocationsQuery(Guid UserId) : IRequest<IEnumerable<object>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_USER_LOCATIONS_LIST_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
