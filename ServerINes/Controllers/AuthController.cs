using INest.Constants;
using INest.Models.DTOs.Auth;
using INest.Models.Entities;
using INest.Services;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            IAuthService authService,
            UserManager<AppUser> userManager,
            ITokenService tokenService)
        {
            _authService = authService;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                await _authService.SendConfirmationCodeAsync(dto);
                return Ok(new { message = "Код подтверждения отправлен на почту" });
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("confirm-register")]
        public async Task<IActionResult> ConfirmRegister([FromBody] ConfirmRegisterDto dto)
        {
            var success = await _authService.ConfirmRegistrationAsync(dto.Email, dto.Code);
            if (!success) return BadRequest(new { error = "Код неверный или истёк" });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest(new { error = "Пользователь не найден" });

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateJwtToken(user, roles);

            return Ok(new { token, message = "Регистрация завершена" });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);

            if (token == null)
                return Unauthorized(new { error = LacalizationConst.UnconfirmedPassword });

            if (token == "unconfirmed")
                return Unauthorized(new { error = LacalizationConst.UnconfirmedMail });

            return Ok(new { token });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto);
            return Ok(new { message = "Письмо отправлено" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            if (result == null) return BadRequest(new { error = "Пользователь не найден" });
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "Пароль изменен" });
        }

        [HttpPost("logout/{userId}")]
        public async Task<IActionResult> Logout(string userId)
        {
            await _authService.LogoutAsync(userId);
            return Ok(new { message = "Вы вышли" });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var email = User.Identity?.Name;
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            return Ok(new { email, roles });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required");

            var isUnique = await _authService.IsEmailUniqueAsync(email);

            return Ok(new { isUnique });
        }

        [AllowAnonymous]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuthDto dto)
        {
            var token = await _authService.GoogleLoginAsync(dto.IdToken);

            if (token == null)
                return Unauthorized(new { error = "Ошибка авторизации через Google" });

            return Ok(new { token });
        }
    }
}