using MediatR;

namespace INest.Features.Auth.Commands.Register
{
    public record RegisterCommand(
        string Email,
        string Password,
        string Username,
        string? TimeZoneId = null) : IRequest;
}