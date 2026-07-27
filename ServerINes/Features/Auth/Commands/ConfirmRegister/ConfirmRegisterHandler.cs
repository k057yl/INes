using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Features.Auth.DTOs;
using INest.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.ConfirmRegister
{
    public class ConfirmRegisterHandler : IRequestHandler<ConfirmRegisterCommand, AuthResponseDto>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;

        public ConfirmRegisterHandler(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> Handle(ConfirmRegisterCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToUpperInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new AppException(AUTH.ERRORS.USER_NOT_FOUND, 404);

            if (user.VerificationCode != request.Code || user.VerificationCodeExpiryTime < DateTime.UtcNow)
                throw new AppException(AUTH.ERRORS.INVALID_OR_EXPIRED_CODE, 400);

            user.EmailConfirmed = true;
            user.VerificationCode = null;
            user.VerificationCodeExpiryTime = null;

            if (!await _userManager.IsInRoleAsync(user, SharedConstants.DEFAULT_ROLE))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, SharedConstants.DEFAULT_ROLE);
                if (!roleResult.Succeeded) throw new AppException(SYSTEM.DEFAULT_ERROR, 500);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateJwtToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = _tokenService.HashRefreshToken(refreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) throw new AppException(SYSTEM.DEFAULT_ERROR, 500);

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
