using MediatR;

namespace INest.Services.Features.Locations.Commands.DeleteLocation
{
    public record DeleteLocationCommand(Guid UserId, Guid Id) : IRequest<bool>;
}
