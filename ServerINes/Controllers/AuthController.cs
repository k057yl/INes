using INest.Models.DTOs.Auth;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

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

            return Ok(new { message = "Регистрация завершена" });
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
    }
}