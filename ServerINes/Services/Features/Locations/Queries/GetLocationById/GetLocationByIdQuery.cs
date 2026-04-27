using INest.Constants;
using INest.Models.Entities;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Locations.Queries.GetLocationById
{
    public record GetLocationByIdQuery(Guid UserId, Guid LocationId) : IRequest<StorageLocation?>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_LOCATION_DETAIL_KEY(UserId, LocationId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
}
