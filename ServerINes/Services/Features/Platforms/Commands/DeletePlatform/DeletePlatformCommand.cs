using MediatR;

namespace INest.Services.Features.Platforms.Commands.DeletePlatform
{
    public record DeletePlatformCommand(Guid UserId, Guid PlatformId) : IRequest<bool>;
}
