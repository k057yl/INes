using INest.Data.Entities.Finances;
using INest.Features.Platforms.DTOs;
using MediatR;

namespace INest.Features.Platforms.Commands.CreatePlatform
{
    public record CreatePlatformCommand(Guid UserId, PlatformDto Dto) : IRequest<Platform>;
}
