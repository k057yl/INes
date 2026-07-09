using INest.Features.Auth.DTOs;
using MediatR;

namespace INest.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponseDto>;
}
