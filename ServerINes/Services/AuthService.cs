using Google.Apis.Auth;
using INest.Constants;
using INest.Exceptions;
using INest.Models.DTOs.Auth;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace INest.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IStringLocalizer<SharedResource> _emailT;

        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expire)> _otpStore = new();

        public AuthService(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration config,
            IStringLocalizer<SharedResource> emailT)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
            _emailT = emailT;
        }

        public async Task SendConfirmationCodeAsync(RegisterDto dto)
        {
            var normalizedEmail = dto.Email.ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user != null && user.EmailConfirmed)
                throw new AppException(LocalizationConstants.AUTH.EMAIL_ALREADY_EXISTS, 400);

            if (user == null)
            {
                user = new AppUser { Email = normalizedEmail, UserName = dto.Username, EmailConfirmed = false };
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new AppException(errorMsg, 400);
                }
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            _otpStore[normalizedEmail] = (code, DateTime.UtcNow.AddMinutes(10));

            var subject = _emailT[LocalizationConstants.EMAILS.CONFIRM_SUBJECT];
            var body = string.Format(_emailT[LocalizationConstants.EMAILS.CONFIRM_BODY], code);

            await _emailService.SendEmailAsync(normalizedEmail, subject, body);
        }

        public async Task<AuthResponseDto> ConfirmRegistrationAsync(ConfirmRegisterDto dto)
        {
            var email = dto.Email.ToLowerInvariant();

            if (!_otpStore.TryGetValue(email, out var entry) || entry.Code != dto.Code || entry.Expire < DateTime.UtcNow)
                throw new AppException(LocalizationConstants.AUTH.INVALID_OR_EXPIRED_CODE, 400);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new AppException(LocalizationConstants.AUTH.USER_NOT_FOUND, 404);

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            if (!await _userManager.IsInRoleAsync(user, SharedConstants.DEFAULT_ROLE))
                await _userManager.AddToRoleAsync(user, SharedConstants.DEFAULT_ROLE);

            _otpStore.TryRemove(email, out _);

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new AppException(LocalizationConstants.AUTH.INVALID_CREDENTIALS, 401);

            if (!user.EmailConfirmed)
                throw new AppException(LocalizationConstants.AUTH.EMAIL_UNCONFIRMED, 401);

            return await GenerateAuthResponse(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var baseUrl = _config["Frontend:Url"];
            var pathTemplate = _config["Frontend:ResetPasswordPath"]
                               ?? "{0}/reset-password?email={1}&token={2}";

            var callbackUrl = string.Format(pathTemplate, baseUrl, dto.Email, Uri.EscapeDataString(token));

            var subject = _emailT[LocalizationConstants.EMAILS.RESET_SUBJECT];
            var body = string.Format(_emailT[LocalizationConstants.EMAILS.RESET_BODY], callbackUrl);

            await _emailService.SendEmailAsync(dto.Email, subject, body);
        }

        public async Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return null;

            return await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return (await _userManager.FindByEmailAsync(email)) == null;
        }

        public async Task<AuthResponseDto?> GoogleLoginAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                var user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new AppUser { Email = payload.Email, UserName = payload.Email, EmailConfirmed = true };
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, SharedConstants.DEFAULT_ROLE);
                    }
                }

                return await GenerateAuthResponse(user);
            }
            catch (InvalidJwtException)
            {
                throw new AppException(LocalizationConstants.AUTH.GOOGLE_AUTH_FAILED, 400);
            }
            catch (Exception)
            {
                throw new AppException(LocalizationConstants.SYSTEM.DEFAULT_ERROR, 500);
            }
        }

        public async Task LogoutAsync(string userId) => await Task.CompletedTask;

        private async Task<AuthResponseDto> GenerateAuthResponse(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateJwtToken(user, roles);
            return new AuthResponseDto { Token = token };
        }
    }
}