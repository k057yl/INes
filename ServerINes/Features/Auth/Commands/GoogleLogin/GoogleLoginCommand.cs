using INest.Features.Auth.DTOs;
using MediatR;

namespace INest.Features.Auth.Commands.GoogleLogin
{
    public record GoogleLoginCommand(
        string IdToken,
        string? TimeZoneId = null) : IRequest<AuthResponseDto?>;
}