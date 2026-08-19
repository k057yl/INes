using MediatR;

namespace INest.Features.Auth.Commands.DeleteAccount
{
    public record DeleteAccountCommand(string UserId, string? Password) : IRequest<bool>;
}
