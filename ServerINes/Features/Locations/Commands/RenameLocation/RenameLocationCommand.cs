using MediatR;

namespace INest.Features.Locations.Commands.RenameLocation
{
    public record RenameLocationCommand(Guid UserId, Guid LocationId, string NewName) : IRequest<bool>;
}
