using INest.Models.DTOs.Location;
using INest.Models.Entities;
using MediatR;

namespace INest.Services.Features.Locations.Commands.CreateLocation
{
    public record CreateLocationCommand(Guid UserId, CreateLocationDto Dto) : IRequest<StorageLocation>;
}
