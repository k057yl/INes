using MediatR;

namespace INest.Services.Features.Locations.Commands.ReorderLocations
{
    public record ReorderLocationsCommand(Guid UserId, Guid? ParentId, List<Guid> OrderedIds) : IRequest<bool>;
}
