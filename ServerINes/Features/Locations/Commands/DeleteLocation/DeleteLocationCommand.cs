using MediatR;

namespace INest.Features.Locations.Commands.DeleteLocation
{
    public record DeleteLocationCommand(Guid Id, Guid UserId, Guid? TargetLocationId = null) : IRequest<bool>;
}
