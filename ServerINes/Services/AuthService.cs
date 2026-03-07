using INest.Models.DTOs.Auth;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using Google.Apis.Auth;

namespace INest.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expire)> _otpStore = new();

        public AuthService(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration config)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
        }

        public async Task SendConfirmationCodeAsync(RegisterDto dto)
        {
            var normalizedEmail = dto.Email.ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user != null && user.EmailConfirmed)
                throw new InvalidOperationException("EMAIL_ALREADY_EXISTS");

            if (user == null)
            {
                user = new AppUser { Email = normalizedEmail, UserName = dto.Username, EmailConfirmed = false };
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded) throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var code = new Random().Next(100000, 999999).ToString();
            _otpStore[normalizedEmail] = (code, DateTime.UtcNow.AddMinutes(10));

            Console.WriteLine($"[DEBUG] OTP for {normalizedEmail}: {code}");

            var html = $"<h3>Welcome to INest!</h3><p>Your confirmation code: <b>{code}</b></p>";
            await _emailService.SendEmailAsync(normalizedEmail, "Confirm your registration", html);
        }

        public async Task<AuthResponseDto?> ConfirmRegistrationAsync(ConfirmRegisterDto dto)
        {
            var email = dto.Email.ToLowerInvariant();
            if (!_otpStore.TryGetValue(email, out var entry) || entry.Code != dto.Code || entry.Expire < DateTime.UtcNow)
                return null;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            if (!await _userManager.IsInRoleAsync(user, "inest_app_user"))
                await _userManager.AddToRoleAsync(user, "inest_app_user");

            _otpStore.TryRemove(email, out _);

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return new AuthResponseDto { Success = false, Message = "INVALID_CREDENTIALS" };

            if (!user.EmailConfirmed)
                return new AuthResponseDto { Success = false, IsEmailConfirmed = false, Message = "EMAIL_UNCONFIRMED" };

            return await GenerateAuthResponse(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{_config["Frontend:Url"]}/reset-password?email={dto.Email}&token={Uri.EscapeDataString(token)}";

            var html = $"<p>Click <a href='{callbackUrl}'>here</a> to reset your password.</p>";
            await _emailService.SendEmailAsync(dto.Email, "Reset Password", html);
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
                var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { _config["Google:ClientId"] } };
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                var user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new AppUser { Email = payload.Email, UserName = payload.Email, EmailConfirmed = true };
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "inest_app_user");
                }

                return await GenerateAuthResponse(user);
            }
            catch { return null; }
        }

        public async Task LogoutAsync(string userId) => await Task.CompletedTask;

        private async Task<AuthResponseDto> GenerateAuthResponse(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateJwtToken(user, roles);
            return new AuthResponseDto { Token = token, Success = true };
        }
    }
}