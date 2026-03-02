using INest.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace INest.Services.Interfaces
{
    public interface IAuthService
    {
        Task SendConfirmationCodeAsync(RegisterDto dto);
        Task<bool> ConfirmRegistrationAsync(string email, string code);
        Task<string?> LoginAsync(LoginDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDto dto);
        Task LogoutAsync(string userId);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<string?> GoogleLoginAsync(string idToken);
    }
}
