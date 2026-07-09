using MediatR;

namespace INest.Features.Auth.Commands.ResendCode
{
    public record ResendCodeCommand(string Email) : IRequest;
}
