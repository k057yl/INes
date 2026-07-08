using INest.Data.Entities.Finances;
using INest.Models.DTOs.Platform;
using MediatR;

namespace INest.Services.Features.Platforms.Commands.UpdatePlatform
{
    public record UpdatePlatformCommand(Guid UserId, Guid PlatformId, PlatformDto Dto) : IRequest<Platform>;
}
