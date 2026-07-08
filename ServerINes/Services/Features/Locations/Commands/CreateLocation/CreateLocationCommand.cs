using INest.Data.Entities.Core;
using INest.Models.DTOs.Location;
using MediatR;

namespace INest.Services.Features.Locations.Commands.CreateLocation
{
    public record CreateLocationCommand(Guid UserId, CreateLocationDto Dto) : IRequest<StorageLocation>;
}
