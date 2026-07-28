using INest.Features.Items.DTOs;
using MediatR;

namespace INest.Features.Locations.Queries.GetLocationItems
{
    public record GetLocationItemsQuery(Guid UserId, Guid LocationId) : IRequest<IEnumerable<ItemDto>>;
}
