using MediatR;
using Microsoft.AspNetCore.Identity;

namespace INest.Features.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<IdentityResult?>;
}
