using MediatR;

namespace INest.Features.Platforms.Commands.DeletePlatform
{
    public record DeletePlatformCommand(Guid UserId, Guid PlatformId) : IRequest<bool>;
}
