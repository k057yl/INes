using INest.Models.DTOs.Platform;
using INest.Models.Entities;
using MediatR;

namespace INest.Services.Features.Platforms.Commands.CreatePlatform
{
    public record CreatePlatformCommand(Guid UserId, PlatformDto Dto) : IRequest<Platform>;
}
