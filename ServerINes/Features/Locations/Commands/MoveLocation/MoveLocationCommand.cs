using MediatR;

namespace INest.Features.Locations.Commands.MoveLocation
{
    public record MoveLocationCommand(Guid UserId, Guid LocationId, Guid? NewParentId) : IRequest<bool>;
}
