using Google.Apis.Auth;
using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Features.Auth.DTOs;
using INest.Infrastructure.Identity;
using INest.Infrastructure.Sanitizer;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.GoogleLogin
{
    public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, AuthResponseDto?>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly ISanitizerService _sanitizer;

        public GoogleLoginHandler(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IConfiguration config,
            ISanitizerService sanitizer)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _config = config;
            _sanitizer = sanitizer;
        }

        public async Task<AuthResponseDto?> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                var user = await _userManager.FindByEmailAsync(payload.Email);

                var sanitizedTimeZone = string.IsNullOrWhiteSpace(request.TimeZoneId)
                    ? "Europe/Kyiv"
                    : _sanitizer.StripAllHtml(request.TimeZoneId);

                if (user == null)
                {
                    var rawName = payload.Name ?? payload.Email.Split('@')[0];
                    var sanitizedDisplayName = _sanitizer.StripAllHtml(rawName);

                    user = new AppUser
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        DisplayName = string.IsNullOrWhiteSpace(sanitizedDisplayName) ? "User" : sanitizedDisplayName,
                        EmailConfirmed = true,
                        TimeZoneId = sanitizedTimeZone
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, SharedConstants.DEFAULT_ROLE);
                    }
                }
                else
                {
                    if (user.TimeZoneId != sanitizedTimeZone)
                    {
                        user.TimeZoneId = sanitizedTimeZone;
                    }
                }

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
            catch (InvalidJwtException)
            {
                throw new AppException(AUTH.ERRORS.GOOGLE_AUTH_FAILED, 400);
            }
            catch (Exception)
            {
                throw new AppException(SYSTEM.DEFAULT_ERROR, 500);
            }
        }
    }
}