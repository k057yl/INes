using INest.Constants;
using INest.Data.Entities.Core;
using INest.Features.Locations.DTOs;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Locations.Queries.GetLocationTree
{
    public record GetLocationTreeQuery(Guid UserId) : IRequest<List<StorageLocationTreeDto>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_LOCATIONS_TREE_KEY(UserId);

        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
