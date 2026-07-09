using INest.Data.Entities.Core;
using INest.Features.Locations.DTOs;
using MediatR;

namespace INest.Features.Locations.Commands.CreateLocation
{
    public record CreateLocationCommand(Guid UserId, CreateLocationDto Dto) : IRequest<StorageLocation>;
}
