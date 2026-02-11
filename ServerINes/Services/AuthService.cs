using INest.Models.DTOs.Auth;
using INest.Models.Entities;
using INest.Resources;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;

namespace INest.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        private readonly IStringLocalizer<SharedResource> _localizer;

        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expire)> _otpStore
            = new();

        public AuthService(UserManager<AppUser> userManager, ITokenService tokenService, IEmailService emailService,
            IConfiguration config, IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;

            _localizer = localizer;
        }

        public async Task SendConfirmationCodeAsync(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException(_localizer["RequiredAuthFields"]);

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Пользователь с таким email уже существует");

            var user = new AppUser
            {
                Email = dto.Email,
                UserName = dto.Username,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Ошибка создания пользователя");

            var code = new Random().Next(100000, 999999).ToString();
            var expire = DateTime.UtcNow.AddMinutes(3);
            _otpStore[dto.Email] = (code, expire);

            var html = $"<p>Ваш одноразовый код подтверждения регистрации: <b>{code}</b></p>";
            await _emailService.SendEmailAsync(dto.Email!, "Подтверждение регистрации", html);
        }

        public async Task<bool> ConfirmRegistrationAsync(string email, string code)
        {
            if (!_otpStore.TryGetValue(email, out var entry))
                return false;

            if (entry.Code != code || entry.Expire < DateTime.UtcNow)
                return false;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            _otpStore.TryRemove(email, out _);
            return true;
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
    }
}