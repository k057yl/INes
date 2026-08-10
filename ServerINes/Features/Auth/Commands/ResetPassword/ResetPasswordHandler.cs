using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static INest.Constants.LocalizationConstants;

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
            var cleanEmail = request.Email.Trim();
            var cleanToken = request.Token?.Trim();

            var user = await _userManager.FindByEmailAsync(cleanEmail);
            if (user == null) return null;

            if (user.VerificationCode != cleanToken || user.VerificationCodeExpiryTime <= DateTime.UtcNow)
            {
                throw new AppException(AUTH.ERRORS.INVALID_OR_EXPIRED_CODE, 400);
            }

            var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, identityResetToken, request.NewPassword);

            if (result.Succeeded)
            {
                user.VerificationCode = null;
                user.VerificationCodeExpiryTime = null;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }
    }
}