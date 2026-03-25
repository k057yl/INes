using INest.Models.DTOs.Auth;
using INest.Models.DTOs.Token;
using Microsoft.AspNetCore.Identity;

namespace INest.Services.Interfaces
{
    public interface IAuthService
    {
        Task SendConfirmationCodeAsync(RegisterDto dto);
        Task<AuthResponseDto> ConfirmRegistrationAsync(ConfirmRegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDto dto);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<AuthResponseDto?> GoogleLoginAsync(string idToken);
        Task LogoutAsync(string userId);
        Task<AuthResponseDto> RefreshTokenAsync(TokenRequestDto dto);
    }
}