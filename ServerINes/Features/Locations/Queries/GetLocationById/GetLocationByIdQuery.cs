using INest.Constants;
using INest.Features.Locations.DTOs;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Locations.Queries.GetLocationById
{
    public record GetLocationByIdQuery(Guid UserId, Guid LocationId) : IRequest<StorageLocationDetailDto?>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_LOCATION_DETAIL_KEY(UserId, LocationId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
}
