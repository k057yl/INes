using MediatR;

namespace INest.Services.Features.Locations.Commands.DeleteLocation
{
    public record DeleteLocationCommand(Guid UserId, Guid LocationId) : IRequest<bool>;
}
