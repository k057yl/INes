using INest.Constants;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Locations.Queries.GetLocations
{
    public record GetLocationsQuery(Guid UserId) : IRequest<IEnumerable<object>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_USER_LOCATIONS_LIST_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
