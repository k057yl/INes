using INest.Data.Entities.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace INest.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, IdentityResult?>
    {
        private readonly UserManager<AppUser> _userManager;

        public ResetPasswordHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult?> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return null;

            return await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        }
    }
}
