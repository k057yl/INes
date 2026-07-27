using MediatR;

namespace INest.Features.Auth.Commands.Register
{
    public record RegisterCommand(
        string Username,
        string Email,
        string Password,
        string? TimeZoneId = null) : IRequest;
}