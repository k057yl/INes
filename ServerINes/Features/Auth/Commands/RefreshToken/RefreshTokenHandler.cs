using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Features.Auth.DTOs;
using INest.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;

        public RefreshTokenHandler(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                throw new AppException(AUTH.ERRORS.INVALID_TOKEN, 401);

            var user = await _userManager.FindByIdAsync(userId);
            var hashedInputToken = _tokenService.HashRefreshToken(request.RefreshToken);

            if (user == null || user.RefreshToken != hashedInputToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new AppException(AUTH.ERRORS.INVALID_OR_EXPIRED_CODE, 401);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateJwtToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = _tokenService.HashRefreshToken(refreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
