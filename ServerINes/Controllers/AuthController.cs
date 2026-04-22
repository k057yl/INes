using INest.Constants;
using INest.Models.DTOs.Auth;
using INest.Models.DTOs.Token;
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

        private void SetTokenCookies(AuthResponseDto authResponse)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("X-Access-Token", authResponse.Token, cookieOptions);
            Response.Cookies.Append("X-Refresh-Token", authResponse.RefreshToken, cookieOptions);
        }

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
            SetTokenCookies(result);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            SetTokenCookies(result);
            return Ok();
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
            if (result == null) return Unauthorized(new { error = LocalizationConstants.AUTH.GOOGLE_AUTH_FAILED });

            SetTokenCookies(result);
            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("X-Access-Token");
            Response.Cookies.Delete("X-Refresh-Token");
            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var accessToken = Request.Cookies["X-Access-Token"];
            var refreshToken = Request.Cookies["X-Refresh-Token"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var dto = new TokenRequestDto { AccessToken = accessToken, RefreshToken = refreshToken };
            var response = await _authService.RefreshTokenAsync(dto);

            SetTokenCookies(response);
            return Ok();
        }
    }
}