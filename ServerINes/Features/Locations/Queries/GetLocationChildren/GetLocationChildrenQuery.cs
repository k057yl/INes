using INest.Features.Locations.DTOs;
using MediatR;

namespace INest.Features.Locations.Queries.GetLocationChildren
{
    public record GetLocationChildrenQuery(Guid UserId, Guid LocationId) : IRequest<IEnumerable<LocationChildDto>>;
}
