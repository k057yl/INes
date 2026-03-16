using INest.Constants;
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
                return Ok(new { message = LocalizationConstants.AUTH.OTP_SENT });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("confirm-register")]
        public async Task<IActionResult> ConfirmRegister([FromBody] ConfirmRegisterDto dto)
        {
            var result = await _authService.ConfirmRegistrationAsync(dto);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto);
            return Ok(new { message = LocalizationConstants.AUTH.RESET_EMAIL_SENT });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            if (result == null) return NotFound(new { error = LocalizationConstants.AUTH.USER_NOT_FOUND });
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });

            return Ok(new { message = LocalizationConstants.AUTH.PASSWORD_CHANGED });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                email = User.Identity?.Name,
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
            });
        }

        [AllowAnonymous]
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var isUnique = await _authService.IsEmailUniqueAsync(email);
            return Ok(new { isUnique });
        }

        [AllowAnonymous]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuthDto dto)
        {
            var result = await _authService.GoogleLoginAsync(dto.IdToken);
            return result != null ? Ok(result) : Unauthorized(new { error = LocalizationConstants.AUTH.GOOGLE_AUTH_FAILED });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout() => Ok();
    }
}