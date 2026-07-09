using MediatR;

namespace INest.Features.Auth.Commands.Logout
{
    public record LogoutCommand(string UserId) : IRequest;
}
