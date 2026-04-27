using INest.Constants;
using INest.Models.Entities;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Locations.Queries.GetLocationTree
{
    public record GetLocationTreeQuery(Guid UserId) : IRequest<List<StorageLocation>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_LOCATIONS_TREE_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
