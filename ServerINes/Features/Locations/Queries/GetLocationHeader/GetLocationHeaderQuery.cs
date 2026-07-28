using INest.Features.Locations.DTOs;
using MediatR;

namespace INest.Features.Locations.Queries.GetLocationHeader
{
    public record GetLocationHeaderQuery(Guid UserId, Guid LocationId) : IRequest<LocationHeaderDto?>;
}
