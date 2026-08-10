using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Features.Auth.DTOs;
using INest.Infrastructure.Identity;
using INest.Infrastructure.Sanitizer;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ISanitizerService _sanitizer;

        public LoginHandler(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            ISanitizerService sanitizer)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _sanitizer = sanitizer;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var sanitizedEmail = _sanitizer.StripAllHtml(request.Email);
            var user = await _userManager.FindByEmailAsync(sanitizedEmail);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new AppException(AUTH.ERRORS.INVALID_CREDENTIALS, 401);

            if (!user.EmailConfirmed)
                throw new AppException(AUTH.ERRORS.EMAIL_UNCONFIRMED, 401);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateJwtToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = _tokenService.HashRefreshToken(refreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

            if (!string.IsNullOrWhiteSpace(request.TimeZoneId) && user.TimeZoneId != request.TimeZoneId)
            {
                user.TimeZoneId = _sanitizer.StripAllHtml(request.TimeZoneId);
            }

            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}