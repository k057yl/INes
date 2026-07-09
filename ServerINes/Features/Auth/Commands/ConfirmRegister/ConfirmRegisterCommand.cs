using INest.Features.Auth.DTOs;
using MediatR;

namespace INest.Features.Auth.Commands.ConfirmRegister
{
    public record ConfirmRegisterCommand(string Email, string Code) : IRequest<AuthResponseDto>;
}
