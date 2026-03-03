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

        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expire)> _otpStore
            = new();

        public AuthService(UserManager<AppUser> userManager, ITokenService tokenService, IEmailService emailService,
            IConfiguration config)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
        }

        public async Task SendConfirmationCodeAsync(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Auth.RequiredFields");

            var normalizedEmail = dto.Email.ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user != null && user.EmailConfirmed)
                throw new InvalidOperationException("Auth.UserAlreadyExists");

            if (user == null)
            {
                user = new AppUser
                {
                    Email = normalizedEmail,
                    UserName = dto.Username,
                    EmailConfirmed = false
                };
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded) throw new InvalidOperationException("Ошибка создания пользователя");
            }

            var code = new Random().Next(100000, 999999).ToString();
            var expire = DateTime.UtcNow.AddMinutes(10);

            _otpStore[normalizedEmail] = (code, expire);

            Console.WriteLine($"DEBUG: OTP для {normalizedEmail} -> {code}");

            var html = $"<p>Ваш код: <b>{code}</b></p>";
            await _emailService.SendEmailAsync(normalizedEmail, "Подтверждение регистрации", html);
        }

        public async Task<bool> ConfirmRegistrationAsync(string email, string code)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            if (!_otpStore.TryGetValue(normalizedEmail, out var entry))
                return false;

            if (entry.Code != code || entry.Expire < DateTime.UtcNow)
                return false;

            var user = await _userManager.FindByEmailAsync(normalizedEmail);
            if (user == null) return false;

            user.EmailConfirmed = true;

            if (!await _userManager.IsInRoleAsync(user, "inest_app_user"))
            {
                await _userManager.AddToRoleAsync(user, "inest_app_user");
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _otpStore.TryRemove(normalizedEmail, out _);
                return true;
            }

            return false;
        }

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return null;

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid) return null;

            if (!user.EmailConfirmed) return "unconfirmed";

            var roles = await _userManager.GetRolesAsync(user);
            return _tokenService.GenerateJwtToken(user, roles);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var urlToken = Uri.EscapeDataString(token);
            var resetUrl = $"{_config["Frontend:Url"]}/reset-password?email={dto.Email}&token={urlToken}";

            var html = $"<p>Чтобы сбросить пароль, перейди по ссылке: <a href=\"{resetUrl}\">Сбросить пароль</a></p>";
            await _emailService.SendEmailAsync(dto.Email, "Сброс пароля", html);
        }

        public async Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return null;

            return await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        }

        public async Task LogoutAsync(string userId) => await Task.CompletedTask;

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var user = await _userManager.FindByEmailAsync(email);
            return user == null;
        }

        public async Task<string?> GoogleLoginAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { _config["Google:ClientId"]! }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new AppUser
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded) return null;

                    await _userManager.AddToRoleAsync(user, "inest_app_user");
                }

                var roles = await _userManager.GetRolesAsync(user);

                return _tokenService.GenerateJwtToken(user, roles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google Login Error: {ex.Message}");
                return null;
            }
        }
    }
}