using MediatR;

namespace INest.Features.Locations.Commands.DeleteLocation
{
    public record DeleteLocationCommand(Guid UserId, Guid Id) : IRequest<bool>;
}
