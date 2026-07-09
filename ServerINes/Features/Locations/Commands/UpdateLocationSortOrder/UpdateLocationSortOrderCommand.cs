using MediatR;

namespace INest.Features.Locations.Commands.UpdateLocationSortOrder
{
    public record UpdateLocationSortOrderCommand(Guid UserId, Guid LocationId, int NewOrder) : IRequest<bool>;
}
