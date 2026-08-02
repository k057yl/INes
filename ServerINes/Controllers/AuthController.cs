using INest.Features.Auth.Commands.ConfirmRegister;
using INest.Features.Auth.Commands.ForgotPassword;
using INest.Features.Auth.Commands.GoogleLogin;
using INest.Features.Auth.Commands.Login;
using INest.Features.Auth.Commands.Logout;
using INest.Features.Auth.Commands.RefreshToken;
using INest.Features.Auth.Commands.Register;
using INest.Features.Auth.Commands.ResendCode;
using INest.Features.Auth.DTOs;
using INest.Features.Auth.Queries.CheckEmail;
using INest.Features.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static INest.Constants.LocalizationConstants;

namespace INest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

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
            await _mediator.Send(new RegisterCommand(dto.Username, dto.Email, dto.Password));
            return Ok(new { message = AUTH.SUCCESS.OTP_SENT });
        }

        [HttpPost("confirm-register")]
        public async Task<IActionResult> ConfirmRegister([FromBody] ConfirmRegisterDto dto)
        {
            var result = await _mediator.Send(new ConfirmRegisterCommand(dto.Email, dto.Code));
            SetTokenCookies(result);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] ResendCodeDto dto)
        {
            await _mediator.Send(new ResendCodeCommand(dto.Email));
            return Ok(new { message = AUTH.SUCCESS.OTP_SENT });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _mediator.Send(new LoginCommand(dto.Email, dto.Password));
            SetTokenCookies(result);
            return Ok(new { data = result, message = "AUTH.SUCCESS.LOGIN" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _mediator.Send(new ForgotPasswordCommand(dto.Email));
            return Ok(new { message = AUTH.SUCCESS.RESET_EMAIL_SENT });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _mediator.Send(new Features.Auth.Commands.ResetPassword.ResetPasswordCommand(dto.Email, dto.Token, dto.NewPassword));
            if (result == null) return NotFound(new { error = AUTH.ERRORS.USER_NOT_FOUND });
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });

            return Ok(new { message = AUTH.SUCCESS.PASSWORD_CHANGED });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _mediator.Send(new GetMeQuery(userId));
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var isUnique = await _mediator.Send(new CheckEmailQuery(email));
            return Ok(new { isUnique });
        }

        [AllowAnonymous]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuthDto dto)
        {
            var result = await _mediator.Send(new GoogleLoginCommand(dto.IdToken));
            if (result == null) return Unauthorized(new { error = AUTH.ERRORS.GOOGLE_AUTH_FAILED });

            SetTokenCookies(result);
            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await _mediator.Send(new LogoutCommand(userId));
            }

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

            var response = await _mediator.Send(new RefreshTokenCommand(accessToken, refreshToken));

            SetTokenCookies(response);
            return Ok();
        }
    }
}