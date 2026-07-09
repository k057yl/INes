using MediatR;

namespace INest.Features.Auth.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest;
}
