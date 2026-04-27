using MediatR;

namespace INest.Services.Features.Locations.Commands.MoveLocation
{
    public record MoveLocationCommand(Guid UserId, Guid LocationId, Guid? NewParentId) : IRequest<bool>;
}
