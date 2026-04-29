using FluentValidation;
using Ganss.Xss;
using Google.Apis.Auth;
using INest.Constants;
using INest.Exceptions;
using INest.Models.DTOs.Auth;
using INest.Models.DTOs.Token;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Security.Cryptography;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Infrastructure
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IStringLocalizer<SharedResource> _emailT;
        private readonly IHtmlSanitizer _sanitizer;

        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<ForgotPasswordDto> _forgotPwdValidator;
        private readonly IValidator<ResetPasswordDto> _resetPwdValidator;

        public AuthService(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration config,
            IStringLocalizer<SharedResource> emailT,
            IHtmlSanitizer sanitizer,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator,
            IValidator<ForgotPasswordDto> forgotPwdValidator,
            IValidator<ResetPasswordDto> resetPwdValidator)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
            _emailT = emailT;
            _sanitizer = sanitizer;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _forgotPwdValidator = forgotPwdValidator;
            _resetPwdValidator = resetPwdValidator;
        }

        public async Task SendConfirmationCodeAsync(RegisterDto dto)
        {
            var valResult = await _registerValidator.ValidateAsync(dto);
            if (!valResult.IsValid) throw new ValidationAppException(valResult.Errors);

            var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
            var sanitizedUsername = _sanitizer.Sanitize(dto.Username).Trim();

            if (string.IsNullOrWhiteSpace(sanitizedUsername))
                throw new AppException(AUTH.ERRORS.INVALID_USERNAME, 400);

            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user != null && user.EmailConfirmed)
                throw new AppException(AUTH.ERRORS.EMAIL_ALREADY_EXISTS, 400);

            if (user == null)
            {
                user = new AppUser
                {
                    Email = normalizedEmail,
                    UserName = normalizedEmail,
                    DisplayName = sanitizedUsername,
                    EmailConfirmed = false
                };
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded) throw new AppException(AUTH.ERRORS.REGISTRATION_FAILED, 400);
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.VerificationCode = code;
            user.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(10);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) throw new AppException(SYSTEM.DEFAULT_ERROR, 500);

            var subject = _emailT[EMAILS.CONFIRM_SUBJECT];
            var body = string.Format(_emailT[EMAILS.CONFIRM_BODY], code);

            await _emailService.SendEmailAsync(normalizedEmail, subject, body);
        }

        public async Task<AuthResponseDto> ConfirmRegistrationAsync(ConfirmRegisterDto dto)
        {
            var email = dto.Email.Trim().ToUpperInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new AppException(AUTH.ERRORS.USER_NOT_FOUND, 404);

            if (user.VerificationCode != dto.Code || user.VerificationCodeExpiryTime < DateTime.UtcNow)
                throw new AppException(AUTH.ERRORS.INVALID_OR_EXPIRED_CODE, 400);

            user.EmailConfirmed = true;
            user.VerificationCode = null;
            user.VerificationCodeExpiryTime = null;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) throw new AppException(SYSTEM.DEFAULT_ERROR, 500);

            if (!await _userManager.IsInRoleAsync(user, SharedConstants.DEFAULT_ROLE))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, SharedConstants.DEFAULT_ROLE);
                if (!roleResult.Succeeded) throw new AppException(SYSTEM.DEFAULT_ERROR, 500);
            }

            return await GenerateAuthResponse(user);
        }

        public async Task ResendConfirmationCodeAsync(string email)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user == null || user.EmailConfirmed)
                throw new AppException(AUTH.ERRORS.USER_NOT_FOUND, 400);

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.VerificationCode = code;
            user.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(10);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) throw new AppException(SYSTEM.DEFAULT_ERROR, 500);

            var subject = _emailT[EMAILS.CONFIRM_SUBJECT];
            var body = string.Format(_emailT[EMAILS.CONFIRM_BODY], code);

            await _emailService.SendEmailAsync(normalizedEmail, subject, body);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var valResult = await _loginValidator.ValidateAsync(dto);
            if (!valResult.IsValid) throw new ValidationAppException(valResult.Errors);

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new AppException(AUTH.ERRORS.INVALID_CREDENTIALS, 401);

            if (!user.EmailConfirmed)
                throw new AppException(AUTH.ERRORS.EMAIL_UNCONFIRMED, 401);

            return await GenerateAuthResponse(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var valResult = await _forgotPwdValidator.ValidateAsync(dto);
            if (!valResult.IsValid) throw new ValidationAppException(valResult.Errors);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var baseUrl = _config["Frontend:Url"];
            var pathTemplate = _config["Frontend:ResetPasswordPath"] ?? SharedConstants.DEFAULT_RESET_PASSWORD_PATH;

            var callbackUrl = string.Format(pathTemplate, baseUrl, dto.Email, Uri.EscapeDataString(token));

            var subject = _emailT[EMAILS.RESET_SUBJECT];
            var body = string.Format(_emailT[EMAILS.RESET_BODY], callbackUrl);

            await _emailService.SendEmailAsync(dto.Email, subject, body);
        }

        public async Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var valResult = await _resetPwdValidator.ValidateAsync(dto);
            if (!valResult.IsValid) throw new ValidationAppException(valResult.Errors);

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
                    user = new AppUser
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        DisplayName = payload.Name ?? payload.Email.Split('@')[0],
                        EmailConfirmed = true
                    };
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
                throw new AppException(AUTH.ERRORS.GOOGLE_AUTH_FAILED, 400);
            }
            catch (Exception)
            {
                throw new AppException(SYSTEM.DEFAULT_ERROR, 500);
            }
        }

        public async Task LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }
        }

        private async Task<AuthResponseDto> GenerateAuthResponse(AppUser user)
        {
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

        public async Task<AuthResponseDto> RefreshTokenAsync(TokenRequestDto dto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                throw new AppException(AUTH.ERRORS.INVALID_TOKEN, 401);

            var user = await _userManager.FindByIdAsync(userId);

            var hashedInputToken = _tokenService.HashRefreshToken(dto.RefreshToken);

            if (user == null || user.RefreshToken != hashedInputToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new AppException(AUTH.ERRORS.INVALID_OR_EXPIRED_CODE, 401);

            return await GenerateAuthResponse(user);
        }
    }
}