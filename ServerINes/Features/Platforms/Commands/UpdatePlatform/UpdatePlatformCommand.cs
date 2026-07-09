using INest.Data.Entities.Finances;
using INest.Features.Platforms.DTOs;
using MediatR;

namespace INest.Features.Platforms.Commands.UpdatePlatform
{
    public record UpdatePlatformCommand(Guid UserId, Guid PlatformId, PlatformDto Dto) : IRequest<Platform>;
}
