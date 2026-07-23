using INest.Features.Auth.DTOs;
using MediatR;

namespace INest.Features.Auth.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password,
        string? TimeZoneId = null) : IRequest<AuthResponseDto>;
}